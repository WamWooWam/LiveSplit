using System;
using System.Globalization;
using System.Windows.Data;

namespace LiveSplit.UI.Race
{
    public class RaceStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is not RaceState state)
                return null;

            return state switch
            {
                RaceState.NotStarted => "Not Started",
                RaceState.InProgress => "In Progress",
                RaceState.Finished => "Finished"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
