using System.Collections.Generic;
using Cave.Logging;

namespace Cave.Console;

sealed class LogAnsiConsoleWriter : LogWriter
{
    #region Private Fields

    readonly AnsiWriter writer;

    #endregion Private Fields

    #region Public Constructors

    public LogAnsiConsoleWriter(AnsiWriter writer) => this.writer = writer;

    #endregion Public Constructors

    #region Public Methods

    public override void Flush() { }

    public override void Write(LogMessage message, IEnumerable<ILogText> items) => writer.Write(items);

    #endregion Public Methods
}
