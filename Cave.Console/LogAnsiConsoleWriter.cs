using System.Collections.Generic;
using Cave.Logging;

namespace Cave.Console;

class LogAnsiConsoleWriter : LogWriter
{
    #region Public Methods

    public override void Flush() { }

    public override void Write(LogMessage message, IEnumerable<ILogText> items) => AnsiConsole.Write(items);

    #endregion Public Methods
}
