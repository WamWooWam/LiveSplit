using LiveSplit.Model;
using System;

namespace LiveSplit.UI.Components
{
    public class BlankSpaceFactory : IComponentFactory
    {
        public string ComponentName => "Blank Space";

        public string Description => "Shows empty space with a configurable size.";

        public ComponentCategory Category => ComponentCategory.Other;

        public IComponent Create(LiveSplitState state) => new BlankSpace();

        public string UpdateName => ComponentName;

        public string XMLURL => "http://livesplit.org/update/Components/update.LiveSplit.BlankSpace.xml";

        public string UpdateURL => "http://livesplit.org/update/";

        public Version Version => Version.Parse("1.8.0");
    }
}
