using RS232DTE.Models.Enums;
using RS232DTE.Helpers;
using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RS232DTE.Models
{
    public class PortProperties : ObservableObject
    {
        private string portName = "None";
        private int baudRate = 150;
        private int bitCount = 7;
        private string terminator = "None";
        private int parityIndex = 0;
        private int stopBitsIndex = 0;
        private int flowControlIndex;
        private readonly int readTimeout = 500;
        private readonly int writeTimeout = 500;

        public string PortName
        {
            get => portName;
            set
            {
                if (value != "Add") SetProperty(ref portName, value);
                else PortNameAdded?.Invoke(this, new EventArgs());

                PortChanged?.Invoke(this, new EventArgs());
            }
        }

        public int BaudRate
        {
            get => baudRate;
            set
            {
                SetProperty(ref baudRate, value);
                PortChanged?.Invoke(this, new EventArgs());
            }
        }
        public int BitCount
        {
            get => bitCount;
            set
            {
                SetProperty(ref bitCount, value);
                PortChanged?.Invoke(this, new EventArgs());
            }
        }
        public string Terminator
        {
            get => terminator; set
            {
                if (value != "Custom") SetProperty(ref terminator, value);
                else TerminatorAdded?.Invoke(this, new EventArgs());

                PortChanged?.Invoke(this, new EventArgs());
            }
        }
        public int ParityIndex
        {
            get => parityIndex;
            set
            {
                SetProperty(ref parityIndex, value);
                PortChanged?.Invoke(this, new EventArgs());
            }
        }
        public int StopBitsIndex
        {
            get => stopBitsIndex;
            set
            {
                SetProperty(ref stopBitsIndex, value);
                PortChanged?.Invoke(this, new EventArgs());
            }
        }
        public int FlowControlIndex
        {
            get => flowControlIndex;
            set
            {
                SetProperty(ref flowControlIndex, value);
                PortChanged?.Invoke(this, new EventArgs());
            }
        }
        public int ReadTimeout => readTimeout;
        public int WriteTimeout => writeTimeout;

        public event EventHandler? PortChanged;
        public event EventHandler? PortNameAdded;
        public event EventHandler? TerminatorAdded;
    }
}
