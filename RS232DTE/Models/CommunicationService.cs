using RS232DTE.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;

namespace RS232DTE.Models
{
    public class CommunicationService
    {
        #region PrivateFields
        private static readonly string pingHeader = "\\p";
        private bool isPinging = false;
        private Stopwatch? pingStopwatch;
        #endregion

        #region Properties
        public IPort? Port { get; set; }
        public event EventHandler? MessageReceived;
        public event EventHandler? PingReceived;
        public bool IsConnected { get; private set; }
        #endregion

        #region PublicMethods
        /// <summary>
        /// Constructs the communication service
        /// </summary>
        public CommunicationService()
        {
            IsConnected = false;
        }

        /// <summary>
        /// Assigns new port validating its properties
        /// </summary>
        /// <param name="port"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void RecreatePort(IPort port)
        {
            if (port is null) throw new ArgumentNullException(nameof(port));

            if (Port is not null)
            {
                if (IsConnected) CloseConnection();
                Port.Dispose();
            }
            ValidatePort(port!);
            Port = port;
        }

        /// <summary>
        /// Tries to open the port
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void Connect()
        {
            if (Port is null) throw new ArgumentNullException($"Parameter {nameof(Port)} cannot be null");

            try
            {
                Port.Open();
                Port.DataReceived += OnDataReceived;
                IsConnected = true;
            }
            catch (Exception err)
            {
                Port.Close();
                IsConnected = false;
                Debug.WriteLine(err);
                throw;
            }
        }

        /// <summary>
        /// Closes the port
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void CloseConnection()
        {
            if (Port == null) throw new ArgumentNullException("Parameter Port cannot be null");

            Port.Close();
            Port.DataReceived -= OnDataReceived;
            IsConnected = false;
        }

        /// <summary>
        /// Writes a message to the port
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task WriteMessage(string message)
        {
            if (Port == null) throw new ArgumentNullException("Parameter Port cannot be null");
            if (!Port.IsOpen) throw new ArgumentException("Port must be opened");

            await Port.WriteLineAsync(message);
        }

        /// <summary>
        /// Starts the ping message and starts waiting for response
        /// </summary>
        /// <returns></returns>
        public async Task Ping()
        {
            pingStopwatch = Stopwatch.StartNew();
            try
            {
                isPinging = true;
                await PingSend();
            }
            catch (Exception err)
            {
                isPinging = false;
                Debug.WriteLine(err);
                throw;
            }
        }

        /// <summary>
        /// Queries the system for all open serial ports
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetAllSerialDevices()
        {
            var serialDevices = new List<string> { "None" };
            try
            {
                var ports = SerialPort.GetPortNames();
                serialDevices.AddRange(ports);

            }
            catch (Exception err)
            {
                Debug.WriteLine("Failed to get serial devices: " + err);
            }
            serialDevices.Add("Add");
            return serialDevices;
        }
        #endregion

        #region EventHandlers
        private async void OnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            try
            {
                if (Port is null) return;
                if (!IsConnected) return;

                var message = Port.ReadLine();
                if (IsPing(message))
                {
                    if (isPinging)
                    {
                        pingStopwatch?.Stop();
                        var time = pingStopwatch?.ElapsedMilliseconds;
                        PingReceived?.Invoke(this, new PingReceivedEventArgs(time ?? -1));
                        isPinging = false;

                    }
                    else await PingSend();
                }
                else
                {
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine("Receiving data failed with: " + err.Message);
            }
        }
        #endregion

        #region PrivateHelpers
        private static bool IsPing(string message) => pingHeader.Equals(message);

        private async Task PingSend()
        {
            if (Port is null) throw new ArgumentNullException($"Parameter {nameof(Port)} cannot be null");
            if (!Port.IsOpen) throw new Exception("Port must be opened to send pings");

            await Port.WriteLineAsync(pingHeader);
        }

        private void ValidatePort(IPort port)
        {
            if (port.PortName is null) throw new ArgumentNullException($"Parameter {nameof(port.PortName)} cannot be null");
            if (port.Terminator is null) throw new ArgumentNullException($"Parameter {nameof(port.Terminator)} cannot be null");
            if (!Helper.ValidateComPort(port.PortName)) throw new ArgumentException($"Parameter {nameof(port.PortName)} is ill-formed");
            if (!Helper.ValidateBaudRate(port.BaudRate)) throw new ArgumentException($"Parameter {nameof(port.BaudRate)} is ill-formed");
            if (!Helper.ValidateBitCount(port.BitCount)) throw new ArgumentException($"Parameter {nameof(port.BitCount)} is ill-formed");
        }
        #endregion
    }
}
