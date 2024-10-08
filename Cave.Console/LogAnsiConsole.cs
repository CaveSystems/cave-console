using System.Text;
using Cave.Logging;

namespace Cave.Console;

/// <summary>Provides logging to the default system console instance.</summary>
public class LogAnsiConsole : LogReceiver
{
    #region Public Constructors

    /// <summary>Initializes a new instance of the <see cref="LogConsole"/> class.</summary>
    public LogAnsiConsole(AnsiWriter writer) : base()
    {
        Writer = new LogAnsiConsoleWriter(writer);
        MessageFormatter.MessageFormat = LogMessageFormatter.DefaultColored;
    }

    #endregion Public Constructors

    #region Public Properties

    /// <summary>Gets or sets the title of the logconsole.</summary>
    public string Title
    {
        get => AnsiConsole.Title;
        set => AnsiConsole.Title = value;
    }

    #endregion Public Properties

    #region Public Methods

    /// <summary>Creates a new logconsole object.</summary>
    /// <param name="writer">
    /// The writer to write to. This defaults to <see cref="AnsiConsole.Output"/> but can be changed to any target. ( <see cref="AnsiConsole.Error"/>,
    /// /dev/ttyX, ...)
    /// </param>
    /// <param name="flags">Flags.</param>
    /// <param name="level">The log level.</param>
    public static LogAnsiConsole StartNew(AnsiWriter? writer = null, LogConsoleFlags flags = LogConsoleFlags.Default, LogLevel level = LogLevel.Information)
    {
        var console = new LogAnsiConsole(writer ?? AnsiConsole.Output)
        {
            Level = level
        };
        console.MessageFormatter.MessageFormat = flags.ToMessageFormat();
        return Start(console);
    }

    /// <summary>Clears the terminal.</summary>
    public void Clear() => AnsiConsole.Clear();

    /// <inheritdoc/>
    public override string ToString() => "LogAnsiConsole[" + Level + "]";

    #endregion Public Methods
}
