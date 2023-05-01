using LiveSplit.Model;
using LiveSplit.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LiveSplit.UI.Race
{

    public partial class RaceViewWindow : Window
    {
        private readonly ITimerModel _timerModel;
        private readonly ISettings _settings;

        private readonly RaceViewWindowViewModel _viewModel;

        private readonly DispatcherTimer _shortUpdateTimer;
        private readonly DispatcherTimer _longUpdateTimer;

        public RaceViewWindow(ITimerModel timerModel, ISettings settings)
        {
            InitializeComponent();
            _timerModel = timerModel;
            _settings = settings;

            _viewModel = new RaceViewWindowViewModel(timerModel, settings);

            _shortUpdateTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, OnShortTimerTick, Dispatcher);
            _longUpdateTimer = new DispatcherTimer(TimeSpan.FromSeconds(10), DispatcherPriority.Background, OnLongTimerTick, Dispatcher);
        }

        private void OnShortTimerTick(object sender, EventArgs e)
        {
            foreach (var provider in _viewModel.Providers)
                foreach (var race in provider.Races)
                    race.Update();
        }

        private async void OnLongTimerTick(object sender, EventArgs e)
        {
            try
            {
                foreach (var provider in _viewModel.Providers)
                    await provider.UpdateAsync();
            }
            catch (Exception) { } // TODO: handle
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var provider in _viewModel.Providers)
                    await provider.UpdateAsync();
            }
            catch (Exception) { } // TODO: handle

            DataContext = _viewModel;
        }
    }
}
