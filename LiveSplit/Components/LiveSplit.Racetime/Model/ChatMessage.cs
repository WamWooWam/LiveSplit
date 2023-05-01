using LiveSplit.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public class MessageDto
    {
        public int version { get; set; }
        public string message { get; set; }
        public string message_plain { get; set; }
        public string bot { get; set; }
        public DateTime? posted_at { get; set; }
        public bool highlight { get; set; }
        public bool is_system { get; set; }
        public string[] errors { get; set; }
        public UserDto user { get; set; }
    }

    public abstract class ChatMessage : RTModelBase<MessageDto>
    {
        protected ChatMessage(MessageDto data) : base(data)
        {
        }

        public abstract MessageType Type { get; }

        public virtual string Message => Data.message ?? "";
        public virtual RacetimeUser User => Data.user != null ? new RacetimeUser(Data.user) : null;
        public DateTime Posted => Data.posted_at ?? DateTime.MaxValue;
        public virtual bool Highlight => Data.highlight;
        public bool IsSystem => Data.is_system;
    }

    public class LiveSplitMessage : ChatMessage
    {
        public LiveSplitMessage(MessageDto data) : base(data)
        {
        }

        public override MessageType Type => MessageType.LiveSplit;

        public override RacetimeUser User => RacetimeUser.LiveSplit;

        public static LiveSplitMessage Create(string msg, bool important)
        {
            var dataroot = new MessageDto
            {
                message = msg,
                user = RacetimeUser.LiveSplit.Data,
                posted_at = DateTime.Now,
                highlight = important,
                is_system = true
            };
            return new LiveSplitMessage(dataroot);
        }
    }

    public class SystemMessage : ChatMessage
    {
        public SystemMessage(MessageDto data) : base(data)
        {
        }

        public override MessageType Type => MessageType.System;
        public override string Message => Data.message_plain ?? Data.message;
        public override RacetimeUser User => RacetimeUser.System;
        public bool IsFinishingMessage => Regex.IsMatch(Message, "(finish|forfeit|comment|done)", RegexOptions.IgnoreCase);
    }

    public class BotMessage : ChatMessage
    {
        public BotMessage(MessageDto data) : base(data)
        {
        }

        public override MessageType Type => MessageType.Bot;
        public override string Message => Data.message_plain ?? Data.message;
        public string BotName => Data.bot;
        public override RacetimeUser User => RacetimeUser.Bot;
    }

    public class UserMessage : ChatMessage
    {
        public UserMessage(MessageDto data) : base(data)
        {
        }

        public override MessageType Type => MessageType.User;
        public override string Message => Data.message_plain ?? Data.message;
    }

    public class ErrorMessage : ChatMessage
    {
        public ErrorMessage(MessageDto data) : base(data)
        {
        }

        public override MessageType Type => MessageType.Error;
        public override bool Highlight => true;
        public override RacetimeUser User => RacetimeUser.System;
        public override string Message
        {
            get
            {
                string msg = "";
                foreach (var s in (Data.errors ?? new string[0]))
                    msg += s + " ";
                return msg;
            }

        }
    }

    public class SplitMessage : ChatMessage
    {
        private JObject _root;
        public SplitMessage(MessageDto data, JObject root) : base(data)
        {
            _root = root;
        }

        public override MessageType Type => MessageType.SplitUpdate;
        public override string Message => Data.message_plain ?? Data.message;
        public SplitUpdate SplitUpdate => new SplitUpdate(_root.ToObject<SplitUpdateDto>());
    }

    public class RaceMessage : ChatMessage
    {
        private JObject _root;
        public RaceMessage(MessageDto data, JObject root) : base(data)
        {
            _root = root;
        }

        public override MessageType Type => MessageType.Race;
        public override string Message => Data.message_plain ?? Data.message;
        public Race Race => new Race(_root["race"].ToObject<RaceDto>());
    }
}
