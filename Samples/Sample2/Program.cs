using Cave.Console;
using Cave.Logging;

namespace Sample2;

class Program
{
    #region Private Methods

    static void Main(string[] args)
    {
#if ANSI
        var console = LogAnsiConsole.StartNew();
#else
        var console = LogConsole.StartNew();
#endif

        foreach (var formatter in new[] { LogMessageFormatter.ShortColored, LogMessageFormatter.DefaultColored, LogMessageFormatter.ExtendedColored, })
        {
            console.MessageFormatter.MessageFormat = formatter;

            var logger = new Logger("Test");
            foreach (var level in Enum.GetValues<LogLevel>())
            {
                logger.Send(level, (IFormattable)$"This is a test message with values: <cyan>integer<default>={1} <cyan>double<reset>={1.23} <cyan>bool<reset>={true}");
            }

            //wait for all loggers
            Logger.Flush();
        }

        //close logging system
        Logger.Close();
    }

    #endregion Private Methods
}
