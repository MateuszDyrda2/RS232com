using System;

namespace RS232DTE.Models
{
    public class PingReceivedEventArgs : EventArgs
    {
        public long Time { get; set; }
        public PingReceivedEventArgs(long time) => Time = time;
    }
}
