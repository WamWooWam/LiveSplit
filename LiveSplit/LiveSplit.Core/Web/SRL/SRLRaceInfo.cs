using LiveSplit.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LiveSplit.Web.SRL
{

    public class SRLRaceInfo : IRaceInfo
    {
        private class RaceInfo
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("game")]
            public Game Game { get; set; }

            [JsonProperty("goal")]
            public string Goal { get; set; }

            [JsonProperty("state")]
            public int State { get; set; }

            [JsonProperty("time")]
            public int Time { get; set; }

            [JsonProperty("numentrants")]
            public int NumEntrants { get; set; }

            [JsonProperty("entrants")]
            public Dictionary<string, Entrant> Entrants { get; set; }
        }

        private class Entrant
        {
            [JsonProperty("displayname")]
            public string DisplayName { get; set; }

            [JsonProperty("place")]
            public int Place { get; set; }

            [JsonProperty("time")]
            public int Time { get; set; }

            [JsonProperty("statetext")]
            public string StateText { get; set; }

            [JsonProperty("twitch")]
            public string Twitch { get; set; }
        }

        private class Game
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("abbrev")]
            public string Abbreviation { get; set; }

            [JsonProperty("popularity")]
            public int Popularity { get; set; }

            [JsonProperty("popularityrank")]
            public int PopularityRank { get; set; }
        }

        private RaceInfo _data;

        public SRLRaceInfo(dynamic data)
        {
            _data = ((JObject)data).ToObject<RaceInfo>();
            foreach (var entrant in _data.Entrants)
            {
                if (entrant.Value.Time >= 0)
                    Finishes++;
                if (entrant.Value.StateText == "Forfeit")
                    Forfeits++;
            }
        }

        public string Id => _data.Id;
        public string GameName => _data.Game.Name;
        public int Finishes { get; set; } = 0;
        public int Forfeits { get; set; } = 0;
        public int NumEntrants => _data.NumEntrants;
        public string Goal => _data.Goal;
        public int State => _data.State;
        public int Starttime => _data.Time;
        public string GameId => _data.Game.Abbreviation;

        public bool IsParticipant(string username)
        {
            var racers = (_data.Entrants).Select(x => x.Key.ToLower());
            return racers.Contains((username ?? "").ToLower());
        }
        
        public IEnumerable<string> LiveStreams
        {
            get
            {
                foreach (var entrant in _data.Entrants.Values)
                {
                    if (entrant.StateText == "Forfeit" || entrant.Time >= 0)
                        continue;

                    yield return entrant.Twitch;
                }
            }
        }
    }
}
