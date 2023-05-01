using LiveSplit.Model;
using LiveSplit.Racetime.Controller;
using LiveSplit.Racetime.Model;
using LiveSplit.Racetime.View;
using LiveSplit.UI.Components;
using LiveSplit.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.Racetime
{
    public class RacetimeAPI : RaceProviderAPI
    {
        protected static readonly Uri BaseUri = new Uri($"{Properties.Resources.PROTOCOL_REST}://{Properties.Resources.DOMAIN}/");
        protected static string racesEndpoint => Properties.Resources.ENDPOINT_RACES;
        private static RacetimeAPI _instance;
        public static RacetimeAPI Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RacetimeAPI();
                return _instance;
            }
        }


        public RacetimeAPI()
        {
            Authenticator = new RacetimeAuthenticator(new RTAuthentificationSettings());
            JoinRace = Join;
            CreateRace = Create;
        }

        public void Join(ITimerModel model, string id)
        {
            var channel = new RacetimeChannel(model.CurrentState, model, (RacetimeSettings)Settings);
            _ = new ChannelForm(channel, id, model.CurrentState.LayoutSettings.AlwaysOnTop);
        }

        public void Warn()
        {

        }

        public void Create(ITimerModel model)
        {
            var psi = new ProcessStartInfo
            {
                FileName = GetUri(Properties.Resources.CREATE_RACE_ADDRESS).AbsoluteUri,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        public IEnumerable<Race> Races { get; set; }

        internal RacetimeAuthenticator Authenticator { get; set; }

        public override string ProviderName => "racetime.gg";

        public override string Username => Authenticator.Identity?.Name;

        protected Uri GetUri(string subUri)
        {
            return new Uri(BaseUri, subUri);
        }

        public override async Task RefreshRacesListAsync()
        {
            //Task.Factory.StartNew(() => RefreshRacesList());

            Races = await GetRacesFromServer();
            RacesRefreshedCallback?.Invoke(this);
        }

        protected void RefreshRacesList()
        {
        }


        protected async Task<Race[]> GetRacesFromServer()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BaseUri.AbsoluteUri + racesEndpoint);
            request.Headers.Add("Authorization", "Bearer " + Authenticator.AccessToken);

            var response = await HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var raceList = new List<Race>();
            var data = JObject.Parse(await response.Content.ReadAsStringAsync());
            var races = data["races"];
            foreach (var race in races)
            {
                raceList.Add(new Race(race.ToObject<RaceDto>()));
            }

            return raceList.ToArray();
        }

        public override IEnumerable<IRaceInfo> GetRaces()
        {
            return Races;
        }

        public override Uri GetGameImageUrl(string id)
        {
            foreach (var race in Races)
            {
                if (race.Data.category.slug == id && race.Data.category.image != null)
                {
                    return new Uri(race.Data.category.image);
                }
            }

            return null;
        }
    }
}
