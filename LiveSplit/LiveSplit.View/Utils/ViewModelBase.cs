using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiveSplit.Utils
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelBase()
        {
            SyncContext = SynchronizationContext.Current;
        }

        protected SynchronizationContext SyncContext { get; }
        protected void OnPropertyChanged(string propertyName)
        {
            SyncContext.Post((o) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)), null);
        }
    }
}
