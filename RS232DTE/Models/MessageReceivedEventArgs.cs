using System;

namespace RS232DTE.Models
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public MessageReceivedEventArgs(string message) => Message = message;
    }
}
