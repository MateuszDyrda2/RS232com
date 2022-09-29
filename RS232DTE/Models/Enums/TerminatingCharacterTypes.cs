namespace RS232DTE.Models.Enums
{
    /// <summary>
    /// Types of endline delimiters ending messages
    /// </summary>
    public enum TerminatingCharacterTypes
    {
        None, // \0
        CR, // \r
        LF, // \n
        CR_LF, // \r\n
        Custom // Custom
    }
}
