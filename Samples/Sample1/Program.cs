using System.Diagnostics;
using Cave;
using Cave.Console;
using Cave.Logging;

namespace LogCollectorSample;

class Program
{
    #region Private Fields

    static int MessagesSent = 0;
    static int SendersReady = 0;
    static ManualResetEventSlim StartEvent = new(false);

    #endregion Private Fields

    #region Private Methods

    static void Main(string[] args)
    {
        var console = LogConsole.StartNew();

        //prepare 3 logger instances for the test
        var sender1 = new Logger("Sender1");
        var sender2 = new Logger("Sender2");
        var sender3 = new Logger("Sender3");
        //create 3 tasks waiting for start signal to send messages
        var task1 = Task.Factory.StartNew(() => SendMessages(sender1));
        var task2 = Task.Factory.StartNew(() => SendMessages(sender2));
        var task3 = Task.Factory.StartNew(() => SendMessages(sender3));
        //wait until all threads are ready
        while (SendersReady < 3) Thread.Sleep(1);
        //set start event for all threads
        StartEvent.Set();
        //wait until all threads are complete
        Task.WaitAll(task1, task2, task3);

        //wait for all loggers
        Logger.Flush();

        //close logging system
        Logger.Close();
    }

    static void SendMessages(Logger logger)
    {
        var logLevelCount = Enum.GetValues<LogLevel>().Length;
        //set ready signal and log message
        Interlocked.Increment(ref SendersReady);
        logger.Notice($"SendMessages to Logger {logger.SenderName} waiting for start signal...");

        //wait for start signal then log message
        StartEvent.Wait();
        logger.Info($"<green>Begin SendMessages to Logger {logger.SenderName}");

        //send 1000 messages
        var start = MonotonicTime.Now;
        for (var i = 0; i < 1000; i++)
        {
            var level = (LogLevel)(i % logLevelCount);
            logger.Send(level, $"<cyan>Message {i}<reset> is an even message: {i % 2 == 0} or an odd message: {i % 2 == 1}");
            //try to send exactly 1 message / ms
            while (MonotonicTime.Now < start + TimeSpan.FromMilliseconds(i)) Thread.Sleep(0);
        }

        logger.Info($"<red>End SendMessages to Logger {logger.SenderName}");
        Interlocked.Add(ref MessagesSent, 3 + 10000);
    }

    #endregion Private Methods
}
