using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Model
{
    public abstract class RTModelBase<T>
    {
        public DateTime Received { get; set; }

        public T Data { get; set; }

        public RTModelBase(T data)
        {
            Data = data;
        }
    }
}
