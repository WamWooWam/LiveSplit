using System;
using System.Linq;

namespace LiveSplit.Updates
{
    public static class Git
    {
        public static readonly string Revision = "";
        public static readonly string Describe = "";
        public static readonly bool IsDirty = true;
        public static readonly string LastTag = "";
        public static readonly int CommitsSinceLastTag = 0;

        public static readonly string Version = new[] { LastTag }
            .Concat(CommitsSinceLastTag > 0 ? new[] { CommitsSinceLastTag.ToString() } : new string[0])
#if DEBUG
            .Concat(new[] { "debug" })
#endif
            .Concat(IsDirty ? new[] { "dirty" } : new string[0])
            .Aggregate((a, b) => a + "-" + b);

        public static readonly string Branch = "netcore";

        public static readonly Uri RevisionUri = new Uri("https://github.com/LiveSplit/LiveSplit/tree/" + (CommitsSinceLastTag > 0 ? Revision : LastTag));
    }
}
