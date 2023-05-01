using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.Updates;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using UpdateManager;

namespace LiveSplit.UI.Components
{
    public interface IRaceProviderFactory : IUpdateable
    {
        RaceProviderAPI Create(ITimerModel model, RaceProviderSettings settings);
        RaceProviderSettings CreateSettings();
    }

    public abstract class RaceProviderAPI
    {
        protected HttpClient HttpClient { get; private set; }

        public RaceProviderAPI()
        {
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add("User-Agent", UpdateHelper.UserAgent);
        }

        public bool IsActivated { get; set; } = true;
        public abstract IEnumerable<IRaceInfo> GetRaces();
        public RacesRefreshedCallback RacesRefreshedCallback;
        public JoinRaceDelegate JoinRace;
        public CreateRaceDelegate CreateRace;
        public abstract Task RefreshRacesListAsync();
        public abstract Uri GetGameImageUrl(string id);
        public abstract string ProviderName { get; }
        public abstract string Username { get; }
        public RaceProviderSettings Settings { get; set; }
    }

    public delegate void RacesRefreshedCallback(RaceProviderAPI api);
    public delegate void JoinRaceDelegate(ITimerModel model, string raceid);
    public delegate void CreateRaceDelegate(ITimerModel model);
}
