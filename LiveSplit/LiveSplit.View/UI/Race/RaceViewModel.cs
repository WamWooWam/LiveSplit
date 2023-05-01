using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.Utils;
using System;
using System.Windows.Input;

namespace LiveSplit.UI.Race
{
    public class RaceViewModel : ViewModelBase
    {
        private readonly RaceProviderViewModel _raceProviderViewModel;
        private readonly RaceProviderAPI _raceProviderApi;
        private readonly RegularTimeFormatter _formatter;

        private IRaceInfo _race;

        public RaceViewModel(
            RaceProviderViewModel raceProviderViewModel,
            RaceProviderAPI raceProviderApi,
            IRaceInfo race,
            ITimerModel timerModel,
            ISettings settings)
        {
            _raceProviderViewModel = raceProviderViewModel;
            _raceProviderApi = raceProviderApi;
            _race = race;
            _formatter = new RegularTimeFormatter();

            JoinCommand = new RelayCommand((o) =>
            {
                if (!race.IsParticipant(_raceProviderApi.Username))
                    settings.RaceViewer.ShowRace(race);
                else
                {
                    _raceProviderApi.JoinRace?.Invoke(timerModel, race.Id);
                }
            });
        }

        public ICommand JoinCommand { get; }

        public string Id => _race.Id;
        public string GameName => _race.GameName;
        public string Goal => _race.Goal.Trim();
        // BUGBUG: Localisation nightmare
        public string EntrantsText => $"{_race.NumEntrants} entrant{(_race.NumEntrants != 1 ? "s" : "")}";
        public Uri ImageUrl => _raceProviderApi.GetGameImageUrl(_race.GameId);
        public RaceState State => (RaceState)_race.State;

        public string Time
        {
            get
            {
                var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                startTime = startTime.AddSeconds(_race.Starttime);

                var timeSpan = TimeStamp.CurrentDateTime - startTime;
                if (timeSpan < TimeSpan.Zero)
                    timeSpan = TimeSpan.Zero;
                return _formatter.Format(timeSpan);
            }
        }

        public string Finished => string.Format("{0}/{1}", _race.Finishes, _race.NumEntrants - _race.Forfeits);

        public void Update(IRaceInfo race = null)
        {
            if (race != null)
                _race = race;

            OnPropertyChanged(nameof(EntrantsText));
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(Finished));
        }
    }
}
