using System;
using LiveSplit.Model;

namespace LiveSplit.Racetime.Model
{
    public class SplitUpdateDto
    {
        public string split_name { get; internal set; }
        public string split_time { get; internal set; }
        public bool is_undo { get; internal set; }
        public bool is_finish { get; internal set; }
        public string user_id { get; internal set; }
    }

    public class SplitUpdate : RTModelBase<SplitUpdateDto>
    {
        public SplitUpdate(SplitUpdateDto data) : base(data)
        {
        }

        public string SplitName => Data.split_name;
        public TimeSpan? SplitTime => Data.split_time == "-" ? null : TimeSpanParser.Parse(Data.split_time);
        public bool IsUndo => Data.is_undo;
        public bool IsFinish => Data.is_finish;
        public string UserID => Data.user_id;
    }
}
