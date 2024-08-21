using System.Text;
using Cave.Logging;

namespace Cave.Console;

/// <summary>Provides logging to the default system console instance.</summary>
public class LogAnsiConsole : LogReceiver
{
    #region Public Constructors

    /// <summary>Initializes a new instance of the <see cref="LogConsole"/> class.</summary>
    public LogAnsiConsole() : base()
    {
        Writer = new LogAnsiConsoleWriter();
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
    /// <param name="flags">Flags.</param>
    /// <param name="level">The log level.</param>
    public static LogAnsiConsole StartNew(LogConsoleFlags flags = LogConsoleFlags.Default, LogLevel level = LogLevel.Information)
    {
        var console = new LogAnsiConsole();
        console.Level = level;
        switch (flags)
        {
            case LogConsoleFlags.Default:
                console.MessageFormatter.MessageFormat = LogMessageFormatter.DefaultColored;
                break;

            case LogConsoleFlags.DefaultShort:
                console.MessageFormatter.MessageFormat = LogMessageFormatter.ShortColored;
                break;

            default:
            {
                //build by flags
                var format = new StringBuilder();
                format.Append("<inverse>{LevelColor}");
                var prefix = "";
                if (flags.HasFlag(LogConsoleFlags.DisplayOneLetterLevel))
                {
                    format.Append($"{prefix}{{ShortLevel}}");
                    prefix = " ";
                }
                if (flags.HasFlag(LogConsoleFlags.DisplayTimeStamp))
                {
                    format.Append($"{prefix}{{DateTime}}");
                    prefix = " ";
                }
                if (flags.HasFlag(LogConsoleFlags.DisplayLongLevel))
                {
                    format.Append($"{prefix}{{Level}}");
                    prefix = " ";
                }
                format.Append($"{prefix}{{Sender}}><reset> {{Content}}");
                if (flags.HasFlag(LogConsoleFlags.DisplaySource))
                {
                    format.Append(" <inverse><blue>@{SourceFile}({SourceLine}): {SourceMember}\n");
                }
                console.MessageFormatter.MessageFormat = format.ToString();
                break;
            }
        }
        return Start(console);
    }

    /// <summary>Clears the terminal.</summary>
    public void Clear() => AnsiConsole.Clear();

    /// <inheritdoc/>
    public override string ToString() => "LogAnsiConsole[" + Level + "]";

    #endregion Public Methods
}
