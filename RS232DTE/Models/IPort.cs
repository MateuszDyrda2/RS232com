using RS232DTE.Models.Enums;
using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace RS232DTE.Models
{
    public interface IPort : IDisposable
    {
        bool IsOpen { get; }
        string PortName { get; set; }
        int BaudRate { get; set; }
        int BitCount { get; set; }
        string Terminator { get; set; }
        ParityTypes Parity { get; set; }
        StopBitsType StopBits { get; set; }
        FlowControlTypes FlowControl { get; set; }
        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }
        event SerialDataReceivedEventHandler DataReceived;


        void Open();
        void Close();
        Task WriteLineAsync(string message);
        string ReadLine();
    }
}
