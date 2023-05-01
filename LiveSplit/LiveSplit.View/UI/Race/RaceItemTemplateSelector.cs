using System.Windows;
using System.Windows.Controls;

namespace LiveSplit.UI.Race
{
    public class RaceItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NotStartedTemplate { get; set; }
        public DataTemplate InProgressTemplate { get; set; }
        public DataTemplate FinishedTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is RaceViewModel viewModel)
            {
                return (viewModel.State) switch
                {
                    RaceState.NotStarted => NotStartedTemplate,
                    RaceState.InProgress => InProgressTemplate,
                    RaceState.Finished => FinishedTemplate,
                    _ => null,
                };
            }

            return base.SelectTemplate(item, container);
        }
    }
}
