﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.Model.Input;
using LiveSplit.Options;
using LiveSplit.Racetime.Model;
using LiveSplit.TimeFormatters;
using LiveSplit.Web;
using Newtonsoft.Json.Linq;

namespace LiveSplit.Racetime.Controller
{
    public class RacetimeChannel
    {

        public const int bufferSize = 20480;
        public const int maxBufferSize = 2097152;
        public readonly int[] reconnectDelays = { 0, 5, 5, 10, 10, 10, 15 };

        public string FullWebRoot => string.Format("{0}://{1}/", Properties.Resources.PROTOCOL_REST, Properties.Resources.DOMAIN);
        public string FullSocketRoot => string.Format("{0}://{1}/", Properties.Resources.PROTOCOL_WEBSOCKET, Properties.Resources.DOMAIN);

        public Race Race { get; set; }
        public UserStatus PersonalStatus
        {
            get
            {
                return GetPersonalStatus(Race);
            }
        }

        protected ITimerModel Model { get; set; }

        private ClientWebSocket ws;
        protected List<ChatMessage> log = new List<ChatMessage>();
        public int ConnectionError { get; set; }
        public bool IsConnected { get; set; }
        public RacetimeSettings Settings { get; set; }
        public string OpenedBy { get; set; }
        public string Username { get; set; }
        public string UserID { get; set; }
        public bool Invited = false;
        public TimeSpan Offset { get; set; }
        public RacetimeAuthenticator Token { get; set; }
        CancellationTokenSource websocket_cts;
        CancellationTokenSource reconnect_cts;

        public RacetimeChannel(LiveSplitState state, ITimerModel model, RacetimeSettings settings)
        {
            Settings = settings;
            reconnect_cts = new CancellationTokenSource();

            Model = model;

            state.OnSplit += State_OnSplit;
            state.OnUndoSplit += State_OnUndoSplit;
            state.OnReset += State_OnReset;
            model.OnPause += Model_OnPause;
        }

        private void Model_OnPause(object sender, EventArgs e)
        {
            Model.Pause();
        }

        protected UserStatus GetPersonalStatus(Race race)
        {
            var u = race?.Entrants?.FirstOrDefault(x => x.Name.ToLower() == RacetimeAPI.Instance.Authenticator.Identity?.Name.ToLower());
            if (u == null)
                return UserStatus.Unknown;
            return u.Status;
        }
        private List<int> Versions = new List<int>();
        private async Task<bool> ReceiveAndProcess()
        {
            WebSocketReceiveResult result;
            string msg;
            byte[] buf = new byte[bufferSize];

            try
            {
                int maxBufferSize = RacetimeChannel.maxBufferSize;
                int read = 0;
                int free = buf.Length;
                do
                {
                    if (free < 1)
                    {
                        var newSize = buf.Length + (bufferSize);
                        if (newSize > maxBufferSize)
                        {
                            throw new InternalBufferOverflowException();
                        }
                        var newBuf = new byte[newSize];
                        Array.Copy(buf, 0, newBuf, 0, read);
                        buf = newBuf;
                        free = buf.Length - read;
                    }
                    result = await ws?.ReceiveAsync(new ArraySegment<byte>(buf, read, free), websocket_cts?.Token ?? CancellationToken.None);
                    if (websocket_cts?.IsCancellationRequested ?? true)
                        return false;
                    read += result.Count;
                    free -= result.Count;
                }
                while (!result.EndOfMessage);


                msg = Encoding.UTF8.GetString(buf, 0, read);
                RawMessageReceived?.Invoke(this, msg);
            }
            catch (InternalBufferOverflowException)
            {
                //flush socket
                while (!(result = await ws?.ReceiveAsync(new ArraySegment<byte>(buf, 0, buf.Length), websocket_cts?.Token ?? CancellationToken.None)).EndOfMessage) ;
                return false;
            }
            catch
            {
                return false;
            }

            IEnumerable<ChatMessage> chatmessages = Parse(JObject.Parse(msg));
            try
            {
                ChatMessage racemessage;
                if (chatmessages.Count() > 1)
                {
                    racemessage = chatmessages.OrderByDescending(x => x.Data.version).FirstOrDefault();
                }
                else
                {
                    racemessage = chatmessages.FirstOrDefault();
                }

                if (racemessage != null)
                {
                    if (racemessage.Type == MessageType.SplitUpdate)
                    {
                        UpdateSplitComparison((SplitMessage)racemessage);
                        MessageReceived?.Invoke(this, chatmessages);
                    }
                    else if (racemessage.Type == MessageType.Race)
                    {
                        if (!Versions.Contains(racemessage.Data.version))
                        {
                            Versions.Add(racemessage.Data.version);
                            UpdateRaceData((RaceMessage)racemessage);
                            MessageReceived?.Invoke(this, chatmessages);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch { }
            return true;
        }


        public async Task RunAsync(string id)
        {
        start:
            if (IsConnected)
            {
                return;
            }
            websocket_cts = new CancellationTokenSource();
            var Authenticator = RacetimeAPI.Instance.Authenticator;

            using (ws = new ClientWebSocket())
            {
                IsConnected = true;

                AuthResult r = await Authenticator.Authorize();
                Username = Authenticator.Identity?.Name;
                UserID = Authenticator.Identity?.ID;
                switch (r)
                {
                    case AuthResult.Success:
                        SendSystemMessage($"Authorization successful. Hello, {Authenticator.Identity?.Name}");
                        Authorized?.Invoke(this, null);
                        break;
                    case AuthResult.Failure:
                        SendSystemMessage(Authenticator.Error, true);
                        AuthenticationFailed?.Invoke(this, new EventArgs());
                        ConnectionError++;
                        goto cleanup;
                    case AuthResult.Cancelled:
                        SendSystemMessage($"Authorization declined by user.");
                        ConnectionError = -1;
                        goto cleanup;
                    case AuthResult.Pending:
                        Authenticator.StopPendingAuthRequest();
                        IsConnected = false;
                        goto start;
                    case AuthResult.Stale:
                        goto cleanup_silent;

                }

                //opening the socket
                ws.Options.SetRequestHeader("Authorization", $"Bearer {Authenticator.AccessToken}");
                Token = Authenticator;
                try
                {
                    await ws.ConnectAsync(new Uri(FullSocketRoot + "ws/o/race/" + id), websocket_cts.Token);
                }
                catch (WebSocketException wex)
                {
                    ConnectionError++;
                    goto cleanup;
                }

                //initial command to sync LiveSplit
                if (ws.State == WebSocketState.Open)
                {

                    ChannelJoined?.Invoke(this, new EventArgs());
                    SendSystemMessage($"Joined Channel '{id}'");
                    try
                    {
                        ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"action\":\"getrace\" }"));
                        await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                        await ReceiveAndProcess();

                    }
                    catch (Exception ex)
                    {
                        SendSystemMessage("Unable to obtain Race information. Try reloading");
                        goto cleanup;
                    }
                    try
                    {
                        var rf = new StandardComparisonGeneratorsFactory();

                        if (ConnectionError >= 0 && Settings.LoadChatHistory) //don't load after every reconnect
                        {
                            SendSystemMessage("Loading chat history...");
                            ArraySegment<byte> otherBytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"action\":\"gethistory\" }"));
                            await ws.SendAsync(otherBytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                            await ReceiveAndProcess();

                        }
                    }
                    catch
                    {
                        SendSystemMessage("Unable to load chat history");
                    }
                }

                ConnectionError = -1;

                while (ws.State == WebSocketState.Open && !websocket_cts.IsCancellationRequested)
                {

                    try
                    {
                        await ReceiveAndProcess();

                    }
                    catch (Exception ex)
                    {
                    }
                }


                switch (ws.State)
                {
                    case WebSocketState.CloseSent:
                    case WebSocketState.CloseReceived:
                    case WebSocketState.Closed:
                        ConnectionError = -1;
                        break;
                    default:
                    case WebSocketState.Aborted:
                        if (!(websocket_cts?.IsCancellationRequested ?? true))
                            ConnectionError++;

                        break;
                }
            }
            ws = null;

        cleanup:
            IsConnected = false;

            if (ConnectionError >= 0)
            {
                SendSystemMessage($"Auto-reconnect in {reconnectDelays[Math.Min(reconnectDelays.Length - 1, ConnectionError)]}s...");
                await Task.Delay(reconnectDelays[Math.Min(reconnectDelays.Length - 1, ConnectionError)] * 1000);
                goto start;
            }
            else
                SendSystemMessage("Disconnect");

            cleanup_silent:
            websocket_cts?.Dispose();
            websocket_cts = null;
            Disconnected?.Invoke(this, new EventArgs());
        }

        public int current_split = 0;

        private void UpdateRaceData(RaceMessage msg)
        {
            //ignore double tap prevention when updating race data
            var m = Model;
            if (m is DoubleTapPrevention)
                m = ((DoubleTapPrevention)Model).InternalModel;


            RaceState r = Race?.State ?? RaceState.Unknown;
            RaceState nr = msg.Race?.State ?? RaceState.Unknown;
            UserStatus u = GetPersonalStatus(Race);
            UserStatus nu = GetPersonalStatus(msg.Race);

            if (msg.Race != null)
            {
                Race = msg.Race;
                UpdateRaceComparisons(Race);
            }


            //update only neccessary if the state of the player and/or the race has changed
            if ((r != nr) || (u != nu))
            {
                //we are (now) part of the race
                if (nu != UserStatus.NotInRace && nu != UserStatus.Unknown)
                {
                    //the race is starting
                    if ((r == RaceState.Open || r == RaceState.OpenInviteOnly) && nr == RaceState.Starting)
                    {
                        m.Start();
                    }

                    //the race is already running and we're not finished, sync the timer
                    else if (nr == RaceState.Started && nu == UserStatus.Racing)
                    {
                        if (m.CurrentState.CurrentPhase == TimerPhase.Ended)
                        {
                            try
                            {
                                m.CurrentState.CurrentSplitIndex = current_split;
                                m.UndoSplit();
                            }
                            catch { }
                        }
                        else if (m.CurrentState.CurrentPhase == TimerPhase.Paused)
                            m.Pause();
                        else if (m.CurrentState.CurrentPhase == TimerPhase.NotRunning)
                        {
                            m.CurrentState.Run.Offset = DateTime.UtcNow.Subtract(Race.StartedAt);
                            m.CurrentState.AdjustedStartTime = m.CurrentState.StartTimeWithOffset = TimeStamp.Now - m.CurrentState.Run.Offset;
                            m.Start();
                        }
                        else
                        {
                            m.CurrentState.Run.Offset = Offset;
                        }

                    }

                    //Nothing has started yet but we just want to prep the countdown
                    else if ((nr == RaceState.Open || nr == RaceState.OpenInviteOnly) && (nr != RaceState.Starting || nr != RaceState.Started))
                    {
                        if (Offset.TotalMilliseconds == 0)
                        {
                            Offset = m.CurrentState.Run.Offset;
                        }
                        m.CurrentState.Run.Offset = Race.StartDelay.Negate();
                        m.CurrentState.AdjustedStartTime = TimeStamp.Now - m.CurrentState.Run.Offset;
                    }
                    else if (nr == RaceState.Started && ((nu == UserStatus.Finished || nu == UserStatus.Forfeit) && (u == UserStatus.Racing || u == UserStatus.Finished)))
                    {
                        try
                        {
                            current_split = m.CurrentState.CurrentSplitIndex;
                            for (int i = 0; i < 300; i++)
                                m.SkipSplit();
                        }
                        catch { }
                        m.Split();
                    }
                }
            }

            StateChanged?.Invoke(this, nr);
            RaceChanged?.Invoke(this, new EventArgs());
            UserListRefreshed?.Invoke(this, new EventArgs());
            GoalChanged?.Invoke(this, new EventArgs());
        }

        private void UpdateSplitComparison(SplitMessage msg)
        {
            SplitUpdate split = msg.SplitUpdate;
            if (split.UserID == UserID)
            {
                return;
            }

            var run = Model.CurrentState.Run;

            var user = Race?.Entrants?.FirstOrDefault(x => x.ID == split.UserID);
            if (user == null)
            {
                return;
            }

            var comparisonName = RacetimeComparisonGenerator.GetRaceComparisonName(user.FullName);
            var segment = run.FirstOrDefault(x => x.Name.Trim().ToLower() == split.SplitName && x.Comparisons[comparisonName][TimingMethod.RealTime] == null);

            if (split.IsUndo)
            {
                segment = run.LastOrDefault(x => x.Name.Trim().ToLower() == split.SplitName && x.Comparisons[comparisonName][TimingMethod.RealTime] != null);
            }
            if (split.IsFinish)
            {
                segment = run.Last();
            }

            if (segment != null)
            {
                var newTime = new Time(segment.Comparisons[comparisonName]);
                newTime[TimingMethod.RealTime] = split.SplitTime;
                segment.Comparisons[comparisonName] = newTime;
            }
        }

        private void UpdateRaceComparisons(Race race)
        {
            try
            {
                foreach(var entrant in race.Entrants)
                {
                    if (entrant.Name != Username)
                        AddComparison(entrant.FullName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        protected void AddComparison(string userName)
        {
            var run = Model.CurrentState.Run;
            var comparisonName = RacetimeComparisonGenerator.GetRaceComparisonName(userName);
            if (run.ComparisonGenerators.All(x => x.Name != comparisonName))
            {
                CompositeComparisons.AddShortComparisonName(comparisonName, Username);
                run.ComparisonGenerators.Add(new RacetimeComparisonGenerator(comparisonName));
            }
        }

        public void RemoveRaceComparisons()
        {
            if (RacetimeComparisonGenerator.IsRaceComparison(Model.CurrentState.CurrentComparison))
                Model.CurrentState.CurrentComparison = Run.PersonalBestComparisonName;

            for (var ind = 0; ind < Model.CurrentState.Run.ComparisonGenerators.Count; ind++)
            {
                if (RacetimeComparisonGenerator.IsRaceComparison(Model.CurrentState.Run.ComparisonGenerators[ind].Name))
                {
                    Model.CurrentState.Run.ComparisonGenerators.RemoveAt(ind);
                    ind--;
                }
            }
            foreach (var segment in Model.CurrentState.Run)
            {
                for (var ind = 0; ind < segment.Comparisons.Count; ind++)
                {
                    var comparison = segment.Comparisons.ElementAt(ind);
                    if (RacetimeComparisonGenerator.IsRaceComparison(comparison.Key))
                        segment.Comparisons[comparison.Key] = default(Time);
                }
            }
        }

        public IEnumerable<ChatMessage> Parse(JObject m)
        {
            switch (m["type"].ToObject<string>())
            {
                case "error":
                    yield return new ErrorMessage(m.ToObject<MessageDto>());
                    break;
                case "race.data":
                    yield return new RaceMessage(m["race"].ToObject<MessageDto>(), m);
                    break;
                case "race.split":
                    yield return new SplitMessage(m["split"].ToObject<MessageDto>(), m);
                    break;
                case "livesplit":
                    yield return new LiveSplitMessage(m["message"].ToObject<MessageDto>());
                    break;
            }
            yield break;
        }



        private void State_OnReset(object sender, TimerPhase value)
        {
            if (PersonalStatus == UserStatus.Racing)
                SendChannelMessage(".forfeit");
        }

        private void State_OnUndoSplit(object sender, EventArgs e)
        {
            if (Model.CurrentState.CurrentSplitIndex == Model.CurrentState.Run.Count - 1)
            {
                if (PersonalStatus != UserStatus.Racing)
                    Undone();
            }

            if (PersonalStatus == UserStatus.Racing)
            {
                var split = Model.CurrentState.CurrentSplit;
                dynamic cmd = new JObject();
                dynamic data = new JObject();
                cmd.action = "split";
                data.split = split.Name;
                data.time = "-";
                cmd.data = data;

                SendChannelCommand(cmd.ToString());
            }
        }

        private void State_OnSplit(object sender, EventArgs e)
        {
            var timeFormatter = new RegularTimeFormatter(TimeAccuracy.Hundredths);
            if (PersonalStatus == UserStatus.Racing)
            {
                if (Model.CurrentState.CurrentSplitIndex > 0)
                {
                    var split = Model.CurrentState.Run[Model.CurrentState.CurrentSplitIndex - 1];
                    dynamic cmd = new JObject();
                    dynamic data = new JObject();
                    cmd.action = "split";
                    data.split = split.Name;
                    data.is_finish = Model.CurrentState.CurrentSplitIndex >= Model.CurrentState.Run.Count ? true : false;
                    data.time = timeFormatter.Format(split.SplitTime.RealTime);
                    cmd.data = data;
                    SendChannelCommand(cmd.ToString());
                }
            }
            if (Model.CurrentState.CurrentSplitIndex >= Model.CurrentState.Run.Count && PersonalStatus == UserStatus.Racing)
                SendChannelMessage(".done");
        }

        public event EventHandler ChannelJoined;
        public event EventHandler Disconnected;
        public event EventHandler GoalChanged;
        public event EventHandler RaceChanged;
        public event EventHandler Kicked;
        public event EventHandler AuthenticationFailed;
        public event EventHandlerT<string> RawMessageReceived;
        public event EventHandlerT<RaceState> StateChanged;
        public event EventHandler UserListRefreshed;
        public event EventHandlerT<IEnumerable<ChatMessage>> MessageReceived;
        public event EventHandler RequestOutputReset;
        public event EventHandlerT<dynamic> DeletedMessage;
        public event EventHandlerT<dynamic> PurgedMessage;
        public event EventHandler Authorized;




        public async void Connect(string id)
        {

            for (int attempts = 0; attempts < 5; attempts++)
            // Lets limit this to 5 tries so we don't try forever.
            {
                try
                {
                    await RunAsync(id.Split('/')[1]);
                    break;
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                }
                // Wait for a few seconds to not spam retry.
                Thread.Sleep(5000);
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                websocket_cts?.Cancel();
                websocket_cts = null;
            }
            reconnect_cts?.Cancel();
            reconnect_cts = null;

            Model.OnPause -= Model_OnPause;
            Model.OnSplit -= State_OnSplit;
            Model.OnReset -= State_OnReset;
            Model.OnUndoSplit -= State_OnUndoSplit;


        }

        public void Forfeit()
        {
            if (PersonalStatus == UserStatus.Racing)
            {
                Model.Reset();
            }
        }


        private Regex cmdRegex = new Regex(@"^\.([a-z]+)\s*?(.+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public JObject CreateCommand(string message)
        {
            dynamic command = new JObject();
            var data = new Dictionary<string, dynamic>();
            Match m = cmdRegex.Match(message);
            if (m.Success)
            {
                string action = m.Groups[1].Value.ToLower().Trim();
                command.action = action;

                if (m.Groups.Count == 3 && m.Groups[2].Value.Trim().Length > 0)
                {
                    data[action] = m.Groups[2].Value.Trim();
                    command.data = data;
                }
            }
            else
            {
                // Default to sending a message action
                command.action = "message";
                data["message"] = message;
                data["guid"] = Guid.NewGuid().ToString();
                command.data = data;
            }

            return command;
        }

        public async void SendChannelCommand(string data)
        {
            RawMessageReceived?.Invoke(this, data);
            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));

            if (IsConnected && ws != null)
            {
                try
                {
                    if (websocket_cts != null)
                        await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, websocket_cts.Token);
                }
                catch
                {
                }
            }
        }

        public void SendChannelMessage(string message)
        {
            message = message.Trim();
            message = message.Replace("\"", "\\\"");

            JObject cmd = CreateCommand(message);
            SendChannelCommand(cmd.ToString());
        }

        public void SendSystemMessage(string message, bool important = false)
        {
            var msg = new ChatMessage[] { LiveSplitMessage.Create(message, important) };
            MessageReceived?.Invoke(this, msg);
            RawMessageReceived?.Invoke(this, msg.First().Posted.ToString());
        }

        public void Ready() => SendChannelMessage(".ready");
        public void Quit() => SendChannelMessage(".quit");
        public void Enter() => SendChannelMessage(".enter");
        public void Accept() => SendChannelMessage(".acceptinvite");
        public void Unready() => SendChannelMessage(".unready");
        public void Done() => Model.Split();
        public void Undone() => SendChannelMessage(".undone");
    }
}
