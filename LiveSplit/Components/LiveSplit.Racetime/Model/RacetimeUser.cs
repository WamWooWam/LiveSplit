using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public class RacetimeUser
    {
        public UserDto Data { get; set; }

        public RacetimeUser(UserDto userDto)
        {
            Data = userDto;
        }

        private int nameChcecksum = -1;
        public int Class
        {
            get
            {
                if (nameChcecksum == -1)
                    nameChcecksum = Name.Sum(x => (int)x);
                return nameChcecksum;
            }
        }
        public override bool Equals(object obj)
        {
            return Name.ToLower() == ((RacetimeUser)obj)?.Name?.ToLower();
        }
        public string ID => Data.id;
        public string FullName => Data.full_name;
        public string Name => Data.name ?? "";
        public string TwitchChannel => Data.twitch_channel;
        public string TwitchName => Data.twitch_name;
        public UserRole Role
        {
            get
            {
                UserRole r = UserRole.Regular;

                if (Data.flair == null)
                    return UserRole.Unknown;

                string[] flairs = Data.flair.ToString().Split(' ');
                foreach (string f in flairs)
                {
                    switch (f)
                    {
                        case "staff": r |= UserRole.Staff; break;
                        case "moderator": r |= UserRole.Moderator; break;
                        case "monitor": r |= UserRole.Monitor; break;
                        case "bot": r |= UserRole.Bot; break;
                        case "system": r |= UserRole.System; break;
                        case "anonymous": r |= UserRole.Anonymous; break;
                    }
                }
                return r;
            }
        }
        public UserStatus Status
        {
            get
            {
                UserStatus s = UserStatus.Unknown;
                if (Data.status == null)
                    return UserStatus.Unknown;

                switch (Data.status.value)
                {
                    case "not_ready": s = UserStatus.NotReady; break;
                    case "ready": s = UserStatus.Ready; break;
                    case "done": s = UserStatus.Finished; break;
                    case "in_progress": s = UserStatus.Racing; break;
                    case "dnf": s = UserStatus.Forfeit; break;
                    case "dq": s = UserStatus.Disqualified; break;
                    default: s = UserStatus.Unknown; break;
                }
                return s;
            }
        }

        public DateTime FinishedAt => Data.finished_at?.ToUniversalTime() ?? DateTime.MaxValue;
        public bool HasFinished => FinishedAt != DateTime.MaxValue;
        public int Place => Data.place ?? 0;
        public string PlaceOrdinal => Data.place_ordinal;
        public string Comment => Data.comment;

        public bool IsLive => Data.stream_live;

        public bool StreamOverride => Data.stream_override;

        public static RacetimeUser System = CreateBot("RaceBot", "bot staff moderator monitor");
        public static RacetimeUser Bot = CreateBot("Bot", "bot staff moderator monitor");
        public static RacetimeUser LiveSplit = CreateBot("LiveSplit", "system staff moderator monitor");
        public static RacetimeUser Anonymous = CreateBot("Anonymous", "anonymous");

        public static RacetimeUser CreateBot(string botname, string flairs)
        {
            return new RacetimeUser(new UserDto() { name = botname, id = botname, flair = flairs});
        }

    }
}
