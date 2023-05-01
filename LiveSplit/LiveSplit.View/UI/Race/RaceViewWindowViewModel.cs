using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.UI.Components;
using System.Collections.ObjectModel;
using System.Linq;

namespace LiveSplit.UI.Race
{
    public class RaceViewWindowViewModel
    {
        private readonly ITimerModel _timerModel;
        private readonly ISettings _settings;

        public RaceViewWindowViewModel(ITimerModel timerModel, ISettings settings)
        {
            _timerModel = timerModel;
            _settings = settings;

            Providers = new ObservableCollection<RaceProviderViewModel>();

            foreach (var providerFactory in ComponentManager.RaceProviderFactories)
            {
                var providerSettings = settings.RaceProvider.FirstOrDefault(x => x.Name == providerFactory.Key);
                var provider = providerFactory.Value.Create(timerModel, providerSettings);

                var providerViewModel = new RaceProviderViewModel(this, provider.ProviderName == "SRL" ? "SpeedRunsLive" : provider.ProviderName, provider);
                Providers.Add(providerViewModel);
            }

            SelectedProvider = Providers.FirstOrDefault();
        }

        public ITimerModel TimerModel => _timerModel;
        public ISettings Settings => _settings;

        public ObservableCollection<RaceProviderViewModel> Providers { get; set; }
        public RaceProviderViewModel SelectedProvider { get; set; }
    }
}
