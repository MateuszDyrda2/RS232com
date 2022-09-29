using RS232DTE.Models.Enums;
using RS232DTE.Models;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;

namespace RS232DTE.Helpers
{
    /// <summary>
    /// Class containing helper methods
    /// </summary>
    public static class Helper
    {
        private static readonly Regex portName = new Regex(@"(?i)COM\s*([0-9]+)", RegexOptions.Compiled);
        public static readonly IEnumerable<int> BitCounts = new List<int> { 7, 8 };
        public static readonly IEnumerable<int> BaudRates = new List<int>
        {
           150, 300, 600, 1200, 1800, 2400, 4800,
           7200, 9600, 14400, 19200, 31250, 38400,
           56000, 57600, 76800, 115200
        };

        /// <summary>
        /// Extension method for conversion between library type and program type
        /// </summary>
        /// <param name="value"></param>
        /// <returns>FlowControlTypes value</returns>
        public static FlowControlTypes Convert(this Handshake value) => value switch
        {
            Handshake.None => FlowControlTypes.None,
            Handshake.XOnXOff => FlowControlTypes.XON_XOFF,
            Handshake.RequestToSend => FlowControlTypes.RTS_CTS,
            Handshake.RequestToSendXOnXOff => FlowControlTypes.None,
            _ => FlowControlTypes.None,
        };

        /// <summary>
        /// Extension method for conversion between library type and program type
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Handshake type</returns>
        public static Handshake Convert(this FlowControlTypes value)
            => value switch
            {
                FlowControlTypes.None => Handshake.None,
                FlowControlTypes.XON_XOFF => Handshake.XOnXOff,
                FlowControlTypes.DTR_DSR => Handshake.None,
                FlowControlTypes.RTS_CTS => Handshake.RequestToSend,
                _ => Handshake.None,
            };

        /// <summary>
        /// Validate that the Port name is correctly set
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool ValidateComPort(string name) => portName.IsMatch(name);

        /// <summary>
        /// Validate that baud rate is one of the predefined
        /// </summary>
        /// <param name="baudRate"></param>
        /// <returns></returns>
        public static bool ValidateBaudRate(int baudRate) => BaudRates.Contains(baudRate);

        /// <summary>
        /// Validate that data size is 7 or 8
        /// </summary>
        /// <param name="bitCount"></param>
        /// <returns></returns>
        public static bool ValidateBitCount(int bitCount) => BitCounts.Contains(bitCount);

        /// <summary>
        /// Convert names of terminators to their string values
        /// </summary>
        /// <param name="terminator"></param>
        /// <returns>Chosen delimiter</returns>
        /// <exception cref="ArgumentException"></exception>
        public static string TerminatorToString(string terminator)
            => terminator switch
            {
                "None" => "\0",
                "CR" => "\r",
                "LF" => "\n",
                "CR_LF" => "\r\n",
                "Custom" => throw new ArgumentException("Incorred terminator"),
                _ => terminator
            };
    }
}
