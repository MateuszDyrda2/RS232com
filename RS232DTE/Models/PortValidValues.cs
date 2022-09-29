using CommunityToolkit.Mvvm.ComponentModel;
using RS232DTE.Helpers;
using RS232DTE.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS232DTE.Models
{
    public class PortValidValues : ObservableObject
    {
        private IEnumerable<string> portNames = new List<string>();
        private IEnumerable<string> terminators = EnumToString<TerminatingCharacterTypes>();

        private readonly IEnumerable<int> bitCounts = new List<int> { 7, 8 };
        private readonly IEnumerable<int> baudRates = new List<int>
        {
           150, 300, 600, 1200, 1800, 2400, 4800,
           7200, 9600, 14400, 19200, 31250, 38400,
           56000, 57600, 76800, 115200
        };
        private readonly IEnumerable<string> flowControls = EnumToString<FlowControlTypes>();
        private readonly IEnumerable<string> parityTypes = EnumToString<ParityTypes>();
        private readonly IEnumerable<string> stopBits = EnumToString<StopBitsType>();

        public IEnumerable<string> Terminators
        {
            get => terminators;
            set => SetProperty(ref terminators, value);
        }

        public IEnumerable<string> PortNames
        {
            get => portNames;
            set => SetProperty(ref portNames, value);
        }

        public IEnumerable<int> BaudRates => baudRates;

        public IEnumerable<int> BitCounts => bitCounts;

        public IEnumerable<string> FlowControls => flowControls;

        public IEnumerable<string> ParityTypes => parityTypes;

        public IEnumerable<string> StopBits => stopBits;

        public static IEnumerable<string> EnumToString<T>()
            where T : Enum => Enum
                                .GetValues(typeof(T))
                                .Cast<T>()
                                .Select(x => x.ToString())
                                .ToList();

    }
}
