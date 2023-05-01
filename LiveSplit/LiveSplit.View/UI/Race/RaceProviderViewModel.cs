using LiveSplit.UI.Components;
using LiveSplit.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace LiveSplit.UI.Race
{
    public class RaceProviderViewModel : ViewModelBase
    {
        private readonly string _name;

        private readonly RaceViewWindowViewModel _owner;
        private readonly RaceProviderAPI _raceProviderApi;

        public RaceProviderViewModel(RaceViewWindowViewModel owner, string name, RaceProviderAPI raceProviderApi)
        {
            _raceProviderApi = raceProviderApi;
            _name = name;
            _owner = owner;

            Races = new ObservableCollection<RaceViewModel>();
            NewRaceCommand = new RelayCommand((o) => _raceProviderApi.CreateRace(owner.TimerModel), (o) => _raceProviderApi.CreateRace != null);

            var racesView = (CollectionView)CollectionViewSource.GetDefaultView(Races);
            racesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(RaceViewModel.State)));
            racesView.SortDescriptions.Add(new SortDescription(nameof(RaceViewModel.State), ListSortDirection.Ascending));
        }

        public string Name { get; set; }
        public ObservableCollection<RaceViewModel> Races { get; set; }
        public ICommand NewRaceCommand { get; set; }

        public async Task UpdateAsync()
        {
            await _raceProviderApi.RefreshRacesListAsync();
            foreach (var race in _raceProviderApi.GetRaces())
            {
                if (race.State != 1 && race.State != 3) continue;

                var raceViewModel = Races.FirstOrDefault(r => r.Id == race.Id);
                if (raceViewModel != null)
                    raceViewModel.Update(race);
                else
                    Races.Add(new RaceViewModel(this, _raceProviderApi, race, _owner.TimerModel, _owner.Settings));
            }

            Name = $"{_name} ({Races.Count})";

            OnPropertyChanged(nameof(Name));
        }
    }
}
