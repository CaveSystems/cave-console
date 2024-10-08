using Cave.Collections;
using Cave.Console;

namespace Sample3.Tty1.Linux;

class Program
{
    #region Private Methods

    static void Main(string[] args)
    {
        using var writer = AnsiWriter.Device($"/dev/tty1");
        writer.WaitUntilNewLine = true;
        writer.Write(Ansi.Control.Title("Sample3.Tty1.Linux"));
        writer.Write(Ansi.ClearScreen);

        AnsiConsole.Output.Write($"Classic Ansi Colors (8 colors)\n");
        foreach (AnsiColor8 bg in Enum.GetValues(typeof(AnsiColor8)))
        {
            foreach (AnsiColor8 fg in Enum.GetValues(typeof(AnsiColor8)))
            {
                writer.Write($"{Ansi.Color(fg, bg)}{fg}{Ansi.Reset} ");
            }
            writer.Write($"|\n");
        }
        writer.Write($"\n");

        writer.Write($"Aixterm Ansi Colors (16 colors)\n");
        foreach (AnsiColor16 bg in Enum.GetValues(typeof(AnsiColor16)))
        {
            foreach (AnsiColor16 fg in Enum.GetValues(typeof(AnsiColor16)))
            {
                writer.Write($"{Ansi.Color(fg, bg)}{fg}{Ansi.Reset} ");
            }
            writer.Write($"|\n");
        }
        writer.Write($"\n");

        writer.Write($"VGA Ansi Colors (256 colors)\n");
        //print only the 216 indexed rgb colors
        var colors = new Counter(0, 6 * 6 * 6).Select(i => (AnsiColor256)(16 + i)).ToList();
        foreach (var bg in colors)
        {
            foreach (var fg in colors)
            {
                writer.Write($"{Ansi.Color(fg, bg)}X");
            }
            writer.Write($"{Ansi.Reset}|\n");
        }
        writer.Write($"\n");
    }

    #endregion Private Methods
}
