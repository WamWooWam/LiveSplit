using System;
using System.Drawing;

namespace WinFormsFontDialog
{
    public class FontChangedEventArgs : EventArgs
    {
        public Font NewFont { get; set; }
    }
}
