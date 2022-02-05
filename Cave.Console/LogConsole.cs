using System;
using Cave.Logging;

namespace Cave.Console
{
    /// <summary>
    /// Provides logging to an <see cref="ILogTarget"/> object.
    /// </summary>
    public class LogConsole : LogReceiver
    {
        #region Private Fields

        string currentTitle;

        #endregion Private Fields

        #region Protected Methods

        /// <summary>
        /// Writes the specified log message.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="level">The level.</param>
        /// <param name="source">The source.</param>
        /// <param name="content">The content.</param>
        protected override void Write(DateTime dateTime, LogLevel level, string source, XT content)
        {
            if (Target == null)
            {
                return;
            }

            lock (Target)
            {
                if (Flags != 0)
                {
                    Target.Inverted = true;

                    #region DisplayLevel

                    if ((Flags & LogConsoleFlags.DisplayLongLevel) != 0)
                    {
                        Target.TextColor = level.GetLogLevelColor();
                        Target.WriteString(StringExtensions.ForceLength(level.ToString(), 12));
                    }
                    if ((Flags & LogConsoleFlags.DisplayOneLetterLevel) != 0)
                    {
                        Target.TextColor = level.GetLogLevelColor();
                        switch (level)
                        {
                            case LogLevel.Emergency: Target.WriteString("!"); break;
                            case LogLevel.Alert: Target.WriteString("A"); break;
                            case LogLevel.Critical: Target.WriteString("C"); break;
                            case LogLevel.Error: Target.WriteString("E"); break;
                            case LogLevel.Warning: Target.WriteString("W"); break;
                            case LogLevel.Notice: Target.WriteString("N"); break;
                            case LogLevel.Information: Target.WriteString("I"); break;
                            case LogLevel.Debug: Target.WriteString("D"); break;
                            case LogLevel.Verbose: Target.WriteString("V"); break;
                            default: Target.WriteString("?"); break;
                        }
                    }

                    #endregion DisplayLevel

                    #region DisplayTimeStamp

                    if ((Flags & LogConsoleFlags.DisplayTimeStamp) != 0)
                    {
                        Target.WriteString(dateTime.ToLocalTime().ToString(DateTimeFormat));
                    }

                    #endregion DisplayTimeStamp

                    #region DisplaySource

                    if ((Flags & LogConsoleFlags.DisplaySource) != 0)
                    {
                        Target.WriteString(" ");
                        Target.WriteString(source);
                    }

                    #endregion DisplaySource

                    Target.ResetColor();
                    Target.WriteString(" ");
                }
                Target.Write(content);
                Target.ResetColor();
                Target.NewLine();
            }
        }

        #endregion Protected Methods

        #region Public Fields

        /// <summary>
        /// The datetime format used to print the messages creation datetime.
        /// </summary>
        public string DateTimeFormat = StringExtensions.ShortTimeFormat;

        /// <summary>
        /// Gets or sets the flags for the logconsole. See the individual flags for more information.
        /// </summary>
        public LogConsoleFlags Flags = LogConsoleFlags.Default;

        #endregion Public Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LogConsole"/> class.
        /// </summary>
        /// <param name="target">The target to log to.</param>
        public LogConsole(ILogTarget target)
        {
            Mode = LogReceiverMode.Continuous;
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Flags = LogConsoleFlags.Default;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>The target.</value>
        public ILogTarget Target { get; private set; }

        /// <summary>
        /// Gets or sets the title of the logconsole.
        /// </summary>
        public string Title
        {
            get => Target.Title;
            set
            {
                currentTitle = new XT(value).ToString();
                Target.Title = currentTitle;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Creates a new <see cref="LogConsole"/> using a loglevel of debug (debug library build) or information (release library build).
        /// </summary>
        public static LogConsole Create(LogLevel level = LogLevel.Information, LogConsoleFlags flags = LogConsoleFlags.Default) => Create(flags, level);

        /// <summary>
        /// Creates a new logconsole object.
        /// </summary>
        /// <param name="flags">Flags.</param>
        /// <param name="level">The log level.</param>
        public static LogConsole Create(LogConsoleFlags flags, LogLevel level = LogLevel.Information)
        {
            var console = new LogSystemConsole();
            if (!flags.HasFlag(LogConsoleFlags.DoNotResetColors))
            {
                console.SetDefaultColors();
            }
            return new(console)
            {
                Flags = flags,
                Level = level,
            };
        }

        /// <summary>
        /// Clears the terminal.
        /// </summary>
        public void Clear() => Target.Clear();

        /// <summary>
        /// Returns LogConsole[Level].
        /// </summary>
        /// <returns></returns>
        public override string ToString() => "LogConsole[" + Level + "]";

        #endregion Public Methods
    }
}
