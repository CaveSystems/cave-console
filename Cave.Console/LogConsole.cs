using System;
using System.Text;
using Cave.Logging;

namespace Cave.Console;

/// <summary>Provides logging to the default system console instance.</summary>
public class LogConsole : LogReceiver, ILogConsole
{
    #region Public Constructors

    /// <summary>Initializes a new instance of the <see cref="LogConsole"/> class.</summary>
    public LogConsole() : base()
    {
        Writer = new LogConsoleWriter();
        MessageFormatter.MessageFormat = LogMessageFormatter.DefaultColored;
    }

    #endregion Public Constructors

    #region Public Properties

    /// <inheritdoc/>
    public string Title
    {
        get => SystemConsole.Title;
        set => SystemConsole.Title = value;
    }

    /// <inheritdoc/>
    public bool IsAnsi => false;

    #endregion Public Properties

    #region Public Methods

    /// <summary>Creates a new logconsole object.</summary>
    /// <param name="flags">Flags.</param>
    /// <param name="level">The log level.</param>
    public static LogConsole StartNew(LogConsoleFlags flags = LogConsoleFlags.Default, LogLevel level = LogLevel.Information)
    {
        var console = new LogConsole
        {
            Level = level
        };
        console.MessageFormatter.MessageFormat = flags.ToMessageFormat();
        return Start(console);
    }

    /// <summary>Clears the terminal.</summary>
    public void Clear()
    {
        SystemConsole.ResetColor();
        SystemConsole.Clear();
    }

    /// <inheritdoc/>
    public override string ToString() => "LogConsole[" + Level + "]";

    #endregion Public Methods
}
