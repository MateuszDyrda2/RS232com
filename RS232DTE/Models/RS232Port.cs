using RS232DTE.Models.Enums;
using System;
using RS232DTE.Helpers;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO.Ports;

namespace RS232DTE.Models
{
    public class RS232Port : IDisposable, IPort
    {
        #region PrivateFields
        private SerialPort port = new SerialPort();
        #endregion

        #region Properties
        public bool IsOpen => port.IsOpen;
        public string PortName
        {
            get => port.PortName;
            set => port.PortName = value;
        }
        public int BaudRate
        {
            get => port.BaudRate;
            set => port.BaudRate = value;
        }
        public int BitCount
        {
            get => port.DataBits;
            set => port.DataBits = value;
        }
        public string Terminator
        {
            get => port.NewLine;
            set => port.NewLine = value;
        }
        public ParityTypes Parity
        {
            get => (ParityTypes)port.Parity;
            set => port.Parity = (Parity)value;
        }
        public StopBitsType StopBits
        {
            get => (StopBitsType)((int)port.StopBits - 1);
            set => port.StopBits = (StopBits)((int)value + 1);
        }
        public FlowControlTypes FlowControl
        {
            get => port.Handshake.Convert();
            set => port.Handshake = value.Convert();
        }
        public int ReadTimeout { get => port.ReadTimeout; set => port.ReadTimeout = value; }
        public int WriteTimeout { get => port.WriteTimeout; set => port.WriteTimeout = value; }
        public event SerialDataReceivedEventHandler DataReceived
        {
            add => port.DataReceived += value;
            remove => port.DataReceived -= value;
        }
        public event SerialPinChangedEventHandler PinChanged
        {
            add => port.PinChanged += value;
            remove => port.PinChanged -= value;
        }

        #endregion

        public RS232Port(
            string portName,
            int baudRate,
            int bitCount,
            string terminator,
            ParityTypes parity,
            StopBitsType stopBits,
            FlowControlTypes flowControl,
            int readTimeout,
            int writeTimeout)
        {
            PortName = portName;
            BaudRate = baudRate;
            BitCount = bitCount;
            Terminator = terminator;
            Parity = parity;
            StopBits = stopBits;
            FlowControl = flowControl;
            ReadTimeout = readTimeout;
            WriteTimeout = writeTimeout;
        }

        #region Communication
        public void Open()
        {
            if (FlowControl == FlowControlTypes.DTR_DSR) port.DtrEnable = true;
            port.Open();
        }

        public void Close()
        {
            if (!IsOpen) return;
            if (FlowControl == FlowControlTypes.DTR_DSR) port.DtrEnable = false;
            port.Close();
        }

        public string ReadLine() => port.ReadLine();

        public async Task WriteLineAsync(string message)
        {
            try
            {
                if (FlowControl == FlowControlTypes.DTR_DSR)
                    await DTRDSRHandshake(WriteTimeout);

                await Task.Run(() => port.WriteLine(message));
            }
            catch (Exception err)
            {
                Debug.WriteLine("Failed to write line: " + err);
                throw;
            }

        }

        #endregion

        #region Dispose
        public void Dispose()
        {
            if (port is not null)
            {
                Close();
                port.Dispose();
            }
        }
        #endregion

        #region PrivateHelpers
        private async Task DTRDSRHandshake(int timeout)
        {
            port.DtrEnable = true;
            var res = await SpinUntilTimeout(
                task: () => port.DsrHolding,
                timeout: timeout,
                delay: 10);

            if (!res) throw new Exception("Handshake timeout");
        }

        private static async Task<bool> SpinUntilTimeout(Func<bool> task, int timeout, int delay)
        {
            bool finished = false;
            int elapsed = 0;
            while ((!finished) && (elapsed < timeout))
            {
                await Task.Delay(delay);
                elapsed += delay;
                finished = task.Invoke();
            }
            return finished;
        }
        #endregion
    }
}
