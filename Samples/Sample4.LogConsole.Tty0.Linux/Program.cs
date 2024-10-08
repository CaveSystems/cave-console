using Cave;
using Cave.Console;
using Cave.Logging;
using Cave.Security;

namespace Sample3.Tty1.Linux;

class Program
{
    #region Private Methods

    static void Log(object? state)
    {
        var level = (LogLevel)(2 + (int)state);
        var log = new Logger($"Logger{state}");
        var watch = StopWatch.StartNew();
        var number = 0;
        while (watch.ElapsedSeconds < 10)
        {
            log.Send(level, $"Message {++number} of state {state}!");
            Thread.Sleep(RNG.UInt16 % 1000);
        }
    }

    static void Main(string[] args)
    {
        try { LogAnsiConsole.StartNew(AnsiWriter.Device($"/dev/tty0"), LogConsoleFlags.DefaultShort); }
        catch (Exception ex)
        {
            AnsiConsole.Error.WriteLine("Could not start ansi writer at /dev/tty0:");
            AnsiConsole.Error.WriteLine(ex.ToLogText(true));
            return;
        }

        var tasks = new List<Task>();
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(Task.Factory.StartNew(Log, i));
        }
        Task.WaitAll(tasks.ToArray());
        Logger.Flush();
    }

    #endregion Private Methods
}
