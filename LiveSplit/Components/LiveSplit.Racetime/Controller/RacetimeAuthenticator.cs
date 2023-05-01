using LiveSplit.Racetime.Model;
using LiveSplit.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Controller
{
    public enum AuthResult { Pending, Success, Failure, Cancelled, Stale }

    public class RacetimeAuthenticator
    {
        protected readonly IAuthentificationSettings s;

        protected string Code { get; set; }
        private TcpListener localEndpoint;
        protected string RedirectUri
        {
            get
            {
                return $"http://{s.RedirectAddress}:{s.RedirectPort}/";
            }
        }


        public string AccessToken
        {
            get { return CredentialManager.ReadCredential("LiveSplit_racetimegg_accesstoken")?.Password; }
            set { CredentialManager.WriteCredential("LiveSplit_racetimegg_accesstoken", "", value); }
        }
        public string RefreshToken
        {
            get { return CredentialManager.ReadCredential("LiveSplit_racetimegg_refreshtoken")?.Password; }
            set { CredentialManager.WriteCredential("LiveSplit_racetimegg_refreshtoken", "", value); }
        }
        public RacetimeUser Identity { get; protected set; }
        public string Error { get; protected set; }
        public DateTime TokenExpireDate { get; protected set; }
        public bool IsAuthenticated
        {
            get
            {
                return Code != null;
            }
        }
        public bool IsAuthorized
        {
            get
            {
                return (AccessToken != null);
            }
        }

        public bool IsAuthorizing { get; set; }

        public void ResetTokens()
        {
            Code = null;
            AccessToken = null;
            TokenExpireDate = DateTime.MaxValue;
        }

        protected async Task<bool> SendRedirectAsync(TcpClient client, string target)
        {
            await Task.Run(() =>
            {
                using (var writer = new StreamWriter(client.GetStream(), new UTF8Encoding(false)))
                {
                    writer.WriteLine("HTTP/1.0 301 Moved Permanently");
                    writer.WriteLine($"Location: {s.AuthServer}{target}");
                }
            });
            return true;
        }


        public RacetimeAuthenticator(IAuthentificationSettings s)
        {
            this.s = s;
            this.HttpClient = new HttpClient();
            this.HttpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        }

        private HttpClient HttpClient { get; set; }

        private readonly Regex parameterRegex = new Regex(@"(\w+)=([-_A-Z0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        private static string ReadResponse(TcpClient client)
        {
            try
            {
                var readBuffer = new byte[client.ReceiveBufferSize];
                string fullServerReply = null;

                using (var inStream = new MemoryStream())
                {
                    var stream = client.GetStream();

                    while (stream.DataAvailable)
                    {
                        var numberOfBytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                        if (numberOfBytesRead <= 0)
                            break;

                        inStream.Write(readBuffer, 0, numberOfBytesRead);
                    }

                    fullServerReply = Encoding.UTF8.GetString(inStream.ToArray());
                }

                return fullServerReply;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string SHA256String(string inputString)
        {
            byte[] bytes = SHA256.HashData(Encoding.ASCII.GetBytes(inputString));
            string base64 = Convert.ToBase64String(bytes);
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            base64 = base64.Replace("=", "");
            return base64;
        }

        private static string GenerateRandomBase64Data(uint length)
        {
            byte[] bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            string base64 = Convert.ToBase64String(bytes);
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            base64 = base64.Replace("=", "");
            return base64;
        }

        private async Task<bool> TryGetUserInfoAsync()
        {
            //try to get the userinfo. if that works we are already authenticated and authorized
            if (AccessToken != null)
            {
                try
                {
                    Identity = await GetUserInfoAsync(s, AccessToken);
                    return Identity != null;
                }
                catch (Exception ex)
                {

                    if (ex.InnerException is SocketException)
                        throw;

                    AccessToken = null;
                    return false;
                }
            }
            return false;
        }

        private async Task<bool> TryRefreshAccess()
        {
            var verifier = GenerateRandomBase64Data(32);

            if (RefreshToken != null)
            {
                var request = $"code={Code}&redirect_uri={RedirectUri}&client_id={s.ClientID}&code_verifier={verifier}&client_secret={s.ClientSecret}&refresh_token={RefreshToken}&grant_type=refresh_token";

                var result = await RestRequest(s.TokenEndpoint, request);
                if (result.Item1 == 200)
                {
                    AccessToken = result.Item2["access_token"].ToObject<string>();
                    RefreshToken = result.Item2["refresh_token"].ToObject<string>();
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> TryGetAccess()
        {
            var verifier = GenerateRandomBase64Data(32);
            var request = $"code={Code}&redirect_uri={RedirectUri}&client_id={s.ClientID}&code_verifier={verifier}&client_secret={s.ClientSecret}&scope={s.Scopes}&grant_type=authorization_code";

            var result = await RestRequest(s.TokenEndpoint, request);
            if (result.Item1 == 400)
            {
                Error = "Access has been revoked. Reauthentication required";
                return false;
            }
            if (result.Item1 != 200)
            {
                Error = "Authentication successful, but access wasn't granted by the server";
                return false;
            }


            AccessToken = result.Item2["access_token"].ToObject<string>();
            RefreshToken = result.Item2["refresh_token"].ToObject<string>();
            TokenExpireDate = DateTime.Now.AddSeconds(result.Item2["expires_in"].ToObject<double>());

            if (AccessToken == null || RefreshToken == null)
            {
                Error = "Final access check failed. Server responded with success, but hasn't delivered a valid Token.";
                return false;
            }
            return true;
        }

        public void StopPendingAuthRequest()
        {
            IsAuthorizing = false;
        }

        public void StopLocalEndpoint()
        {
            if (localEndpoint != null)
            {
                localEndpoint.Server.Close(0);
                localEndpoint.Server.Dispose();
                localEndpoint.Stop();
                localEndpoint = null;
            }
        }

        private async Task<int> TryGetAuthenticated()
        {
            string reqState, state, verifier = null, challenge, request, response;
            Tuple<int, dynamic> result;

            Error = null;
            reqState = null;
            state = GenerateRandomBase64Data(32);
            verifier = GenerateRandomBase64Data(32);
            challenge = SHA256String(verifier);

            try
            {
                StopLocalEndpoint();
                localEndpoint = new TcpListener(s.RedirectAddress, s.RedirectPort);
                localEndpoint.Start();

                request = $"{s.AuthServer}{s.AuthorizationEndpoint}?response_type=code&client_id={s.ClientID}&scope={s.Scopes}&redirect_uri={RedirectUri}&state={state}&code_challenge={challenge}&code_challenge_method={s.ChallengeMethod}";


                Task<TcpClient> serverConnectionTask = localEndpoint.AcceptTcpClientAsync();

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = request, UseShellExecute = true });

                using (TcpClient serverConnection = await serverConnectionTask)
                {
                    response = ReadResponse(serverConnection);

                    StopLocalEndpoint();

                    foreach (Match m in parameterRegex.Matches(response))
                    {
                        switch (m.Groups[1].Value)
                        {
                            case "state": reqState = m.Groups[2].Value; break;
                            case "code": Code = m.Groups[2].Value; break;
                            case "error": Error = m.Groups[2].Value; break;
                        }
                    }

                    if (Error != null)
                    {
                        if (Error == "invalid_token" && RefreshToken != null)
                        {
                            Error = "Access Token expired";
                            return 401;
                        }
                        else
                        {
                            await SendRedirectAsync(serverConnection, s.FailureEndpoint);
                            Error = "Unable to authenticate: The server rejected the request";
                            return 403;
                        }
                    }

                    if (state != reqState)
                    {
                        await SendRedirectAsync(serverConnection, s.FailureEndpoint);
                        Error = "Unable to authenticate: The server hasn't responded correctly. Possible protocol error";
                        return 400;
                    }

                    await SendRedirectAsync(serverConnection, s.SuccessEndpoint);
                    serverConnection.Close();
                }
                StopLocalEndpoint();
            }
            catch (ObjectDisposedException)
            {
                return 404;
            }
            catch (Exception ex)
            {
                Error = "Unknown Error";
                StopLocalEndpoint();
            }

            return Error == null ? 200 : 500;
        }


        public async Task<AuthResult> Authorize()
        {
            if (IsAuthorizing)
                return AuthResult.Pending;

            IsAuthorizing = true;
            Error = null;
            bool secondRun = false;

        start:

            //0th:  ping server to check connectivity
            string host = Properties.Resources.DOMAIN.Contains(":") ? Properties.Resources.DOMAIN.Substring(0, Properties.Resources.DOMAIN.IndexOf(':')) : Properties.Resources.DOMAIN;
            try
            {
                Ping myPing = new Ping();
                byte[] buffer = new byte[32];
                int timeout = 5000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status != IPStatus.Success)
                {
                    Error = $"No Connection to {host}";
                    goto failure;
                }
            }
            catch (Exception)
            {
                Error = $"Unable to ping {host}";
                goto failure;
            }


            //1st: Try to get User information
            try
            {
                if (await TryGetUserInfoAsync())
                    goto success;
            }
            catch (SocketException) { goto failure; }
            catch { }

            //2nd: if this fails, try to renew access 
            try
            {
                //if there is a refresh token
                if (await TryRefreshAccess())
                    goto start;

                //or not
                if (await TryGetAccess())
                    goto start;
            }
            catch { }

            //3rd: everything failed, ask the user to authenticate
            try
            {
                int errorcode = await TryGetAuthenticated();
                switch (errorcode)
                {
                    case 403: goto cancelled;
                    case 200: goto start;
                    case 404: return AuthResult.Stale;
                    default: goto failure;
                }
            }
            catch { }

            //safety safe for a theoretical endless loop
            if (secondRun)
                goto failure;
            secondRun = true;

            if (Error == null)
                goto start;


            failure:
            StopPendingAuthRequest();
            ResetTokens();
            return AuthResult.Failure;
        cancelled:
            StopPendingAuthRequest();
            return AuthResult.Cancelled;
        success:
            StopPendingAuthRequest();
            return AuthResult.Success;
        }

        public async Task<RacetimeUser> GetUserInfoAsync(IAuthentificationSettings s, string AccessToken)
        {
            var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, $"{s.AuthServer}{s.UserInfoEndpoint}");
            userInfoRequest.Headers.Add("Authorization", $"Bearer {AccessToken}");

            using (var resp = await HttpClient.SendAsync(userInfoRequest))
            using (var reader = new StreamReader(await resp.Content.ReadAsStreamAsync()))
            using (var jsonTextReader = new JsonTextReader(reader))
            {
                var userdata = JObject.Load(jsonTextReader);
                return new RacetimeUser(userdata.ToObject<UserDto>());
            }
        }

        public async Task UpdateUserInfoAsync()
        {
            Identity = await GetUserInfoAsync(s, AccessToken);
        }

        public async Task<Tuple<int, JObject>> RestRequest(string endpoint, string body)
        {
            string state = GenerateRandomBase64Data(32);
            body += "&state=" + state;

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{s.AuthServer}{s.TokenEndpoint}");
            tokenRequest.Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

            try
            {
                using (var resp = await HttpClient.SendAsync(tokenRequest))
                using (var reader = new StreamReader(await resp.Content.ReadAsStreamAsync()))
                using (var jsonTextReader = new JsonTextReader(reader))
                {
                    var userdata = JObject.Load(jsonTextReader);
                    return new Tuple<int, JObject>((int)resp.StatusCode, userdata);
                }
            }
            catch
            {
                return new Tuple<int, JObject>(500, null);
            }
        }
    }
}
