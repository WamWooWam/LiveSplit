using LiveSplit.Model;
using LiveSplit.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LiveSplit.Racetime.Model
{
    public class RaceDto
    {
        public bool allow_midrace_chat { get; set; }
        public bool allow_comments { get; set; }
        public string name { get; set; }
        public GoalDto goal { get; set; }
        public string info { get; set; }
        public string start_delay { get; set; }
        public int entrants_count { get; set; }
        public List<EntrantDto> entrants { get; set; }
        public StatusDto status { get; set; }
        public DateTime? started_at { get; set; }
        public DateTime? opened_at { get; set; }
        public UserDto opened_by { get; set; }
        public int entrants_count_finished { get; set; }
        public int entrants_count_inactive { get; set; }
        public CategoryDto category { get; set; }
    }

    public class GoalDto
    {
        public string name { get; set; }
    }

    public class EntrantDto
    {
        public UserDto user { get; set; }
        public StatusDto status { get; set; }
        public int? place { get; set; }
        public string place_ordinal { get; set; }
        public string finish_time { get; set; }
        public string finished_at { get; set; }
        public string start_delay { get; set; }
        public bool stream_live { get; set; }
        public string comment { get; set; }
        public string stream_override { get; set; }
    }

    public class UserDto
    {
        public string id { get; set; }
        public string name { get; set; }
        public string full_name { get; set; }
        public string flair { get; set; }
        public string twitch_name { get; set; }
        public string twitch_channel { get; set; }
        public string discriminator { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? place { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string place_ordinal { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string comment { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool stream_live { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool stream_override { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? finished_at { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public StatusDto status { get; set; }
    }

    public class StatusDto
    {
        public string value { get; set; }
    }

    public class CategoryDto
    {
        public string slug { get; set; }
        public string name { get; set; }
        public string image { get; set; }
    }

    public class Race : RTModelBase<RaceDto>, IRaceInfo
    {
        public Race(RaceDto data) : base(data)
        {
        }

        public bool AllowNonEntrantChat => false;
        public bool AllowMidraceChat => Data.allow_midrace_chat;
        public bool AllowComments => Data.allow_comments;
        public string GameSlug => Id.Substring(0, Id.IndexOf('/'));
        public string Name => Data.name;
        public string Goal => Data.goal?.name;
        public string Info => Data?.info;
        public TimeSpan StartDelay
        {
            get
            {
                try
                {
                    TimeSpan ts = XmlConvert.ToTimeSpan(Data.start_delay);
                    return ts;
                }
                catch
                {
                    return TimeSpan.Zero;
                }
            }
        }
        public int NumEntrants => Data.entrants_count;
        public IEnumerable<RacetimeUser> Entrants =>
            Data.entrants.Select(e => new RacetimeUser(e.user));

        public string ChannelName => Id.Substring(Id.IndexOf('/') + 1);
        public RaceState State
        {
            get
            {
                switch (Data.status.value)
                {
                    case "open": return RaceState.Open;
                    case "invitational": return RaceState.OpenInviteOnly;
                    case "pending": return RaceState.Starting;
                    case "in_progress": return RaceState.Started;
                    case "finished": return RaceState.Ended;
                    case "cancelled": return RaceState.Cancelled;
                    default: return RaceState.Unknown;
                }
            }
        }
        public DateTime StartedAt => Data.started_at ?? DateTime.MaxValue;


        public DateTime OpenedAt => Data.opened_at ?? DateTime.MaxValue;

        public RacetimeUser OpenedBy => new RacetimeUser(Data.opened_by);

        public int Finishes => Data.entrants_count_finished;
        public int Forfeits => Data.entrants_count_inactive;

        public string GameId => Data.category.slug;

        public string GameName => Data.category.name;

        public string Id => Data.name;

        public IEnumerable<string> LiveStreams =>
            Entrants.Where(x => x.Status != UserStatus.Forfeit && x.Status != UserStatus.Disqualified && !x.HasFinished).Select(x => x.TwitchName);

        public int Starttime => 
            StartedAt == DateTime.MaxValue ? 0 : (int)(StartedAt - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        int IRaceInfo.State => 
            (State == RaceState.Open || State == RaceState.OpenInviteOnly) ? 1 : (State == RaceState.Started ? 3 : 42);


        public bool IsParticipant(string username)
        {
            return true;//Entrants.Any(x => x.Name.ToLower() == username?.ToLower());
        }
    }
}
