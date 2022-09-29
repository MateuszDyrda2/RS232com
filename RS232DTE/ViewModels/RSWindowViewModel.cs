using RS232DTE.Helpers;
using RS232DTE.Models;
using RS232DTE.Models.Enums;
using RS232DTE.Views.Components;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace RS232DTE.ViewModels
{
    public class RSWindowViewModel : ViewModelBase
    {
        #region PrivateFields
        private readonly CommunicationService service;
        private Command connectCommand;
        private Command sendCommand;
        private Command clsCommand;
        private Command pingCommand;
        private Command clearOutputCommand;
        private string inputText;
        private string outputText;
        private readonly Dispatcher localDispatcher;
        private string connectionString = ConnectionActions.Connect.ToString();
        #endregion

        #region Properties
        public PortProperties PortProperties { get; set; }
        public PortValidValues PortValues { get; set; }
        public Command ConnectCommand => connectCommand ??= new Command(async x => await Connect(), x => true);
        public Command SendCommand => sendCommand ??= new Command(async x => await Send(), x => true);
        public Command ClsCommand => clsCommand ??= new Command(x => ClearScreen(), x => true);
        public Command PingCommand => pingCommand ??= new Command(async x => await Ping(), x => true);
        public Command ClearOutputCommand => clearOutputCommand ??= new Command(x => ClearOutput(), x => true);
        public string InputText { get => inputText; set => SetProperty(ref inputText, value); }
        public string OutputText { get => outputText; set => SetProperty(ref outputText, value); }
        public string ConnectionString { get => connectionString; set => SetProperty(ref connectionString, value); }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new view model instance
        /// </summary>
        public RSWindowViewModel()
        {
            PortProperties = new PortProperties();
            PortProperties.TerminatorAdded += AddTerminator;
            PortProperties.PortNameAdded += AddPort;
            PortProperties.PortChanged += InvalidateConnection;

            PortValues = new PortValidValues { PortNames = CommunicationService.GetAllSerialDevices() };

            localDispatcher = Dispatcher.CurrentDispatcher;
            try
            {
                service = new CommunicationService();
                service.MessageReceived += OnOutputReceived;
                service.PingReceived += OnPingReceived;
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                ShowErrorMessage(err.Message);
            }
        }
        #endregion

        #region EventHandlers
        /// <summary>
        /// Responds to a ping returns
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPingReceived(object? sender, EventArgs? e)
        {
            try
            {
                if (e == null) throw new ArgumentNullException(nameof(e));

                var result = e as PingReceivedEventArgs;

                localDispatcher.Invoke(new Action(() =>
                {
                    OutputText = "Ping returned in " + result?.Time.ToString() + "ms";
                }));
            }
            catch (Exception err)
            {
                ShowErrorMessage(err.Message);
            }
        }
        /// <summary>
        /// Responds to receiving data 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOutputReceived(object? sender, EventArgs? e)
        {
            try
            {
                if (e == null) throw new ArgumentNullException(nameof(e));

                var message = e as MessageReceivedEventArgs;

                localDispatcher.Invoke(new Action(() =>
                {
                    OutputText = (OutputText ?? string.Empty) + message?.Message;
                }));
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                ShowErrorMessage(err.Message);
            }
        }
        #endregion

        #region Commands
        private Task Connect()
        {
            try
            {
                if (service.IsConnected)
                {
                    ConnectionString = ConnectionActions.Connect.ToString();

                    service.CloseConnection();
                }
                else
                {
                    ConnectionString = ConnectionActions.Disconnect.ToString();

                    service.RecreatePort(RecreatePort());
                    service.Connect();
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine("Error when connecting: " + err.Message);
                InvalidateConnection(null, null);
                ShowErrorMessage(err.Message);
            }

            return Task.CompletedTask;
        }

        private async Task Send()
        {
            try
            {
                if (service == null) throw new ArgumentNullException("Parameter service cannot be null");
                if (!service.IsConnected) throw new Exception("Service must be connected to send messages");
                if (InputText == null && InputText == string.Empty) throw new ArgumentException("Cannot send an empty string");

                var message = InputText;
                await service.WriteMessage(message ?? "Error");

            }
            catch (Exception err)
            {
                Debug.WriteLine("Sending failed with: " + err.Message);
                ShowErrorMessage(err.Message);
            }
        }

        private void ClearScreen() => InputText = string.Empty;

        private void ClearOutput() => OutputText = string.Empty;

        private async Task Ping()
        {
            try
            {
                if (service == null) throw new ArgumentNullException("Parameter service cannot be null");
                if (!service.IsConnected) throw new Exception("Service must be connected to send messages");

                await service.Ping();
            }
            catch (Exception err)
            {
                ShowErrorMessage(err.Message);
            }
        }

        private void AddPort(object? sender, EventArgs? args)
        {
            var dialog = new InputDialog
            {
                Text = "Add Port",
            };

            if (dialog.ShowDialog() == false) return;

            var port = dialog.ResponseText;
            var reg = new Regex(@"(?i)COM\s*([0-9]+)", RegexOptions.Compiled);

            if (!reg.IsMatch(port))
                return;

            var portCleaned = string
                .Concat(port.ToUpper().Where(x => !char.IsWhiteSpace(x)));

            PortValues.PortNames = PortValues.PortNames
                .ToList()
                .Append(portCleaned);
        }

        private void AddTerminator(object? sender, EventArgs? args)
        {
            var dialog = new InputDialog
            {
                Text = "Add Terminating Character",
            };

            if (dialog.ShowDialog() == false) return;

            PortValues.Terminators = PortValues.Terminators
                .ToList()
                .Append(dialog.ResponseText);
        }
        #endregion

        #region PrivateHelpers
        private static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "OK", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void InvalidateConnection(object? sender, EventArgs? args)
        {
            if (ConnectionString == ConnectionActions.Connect.ToString()) return;
            if (service.IsConnected) service.CloseConnection();
            ConnectionString = ConnectionActions.Connect.ToString();
        }

        private RS232Port RecreatePort()
        {
            var port = new RS232Port(
                portName: PortProperties.PortName,
                baudRate: PortProperties.BaudRate,
                bitCount: PortProperties.BitCount,
                terminator: Helper.TerminatorToString(PortProperties.Terminator),
                parity: (ParityTypes)PortProperties.ParityIndex,
                stopBits: (StopBitsType)PortProperties.StopBitsIndex,
                flowControl: (FlowControlTypes)PortProperties.FlowControlIndex,
                readTimeout: PortProperties.ReadTimeout,
                writeTimeout: PortProperties.WriteTimeout);

            port.PinChanged += PortPinChanged;
            return port;
        }

        private void PortPinChanged(object sender, System.IO.Ports.SerialPinChangedEventArgs e)
        {
            localDispatcher.Invoke(new Action(() =>
            {

            }));
        }

        #endregion
    }
}
