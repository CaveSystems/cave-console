using Cave.Collections;
using Cave.Console;

namespace SampleAnsiConsole;

class Program
{
    #region Private Methods

    static void Main(string[] args)
    {
        AnsiConsole.Output.WaitUntilNewLine = true;
        AnsiConsole.Title = "Ansi Console Color Test";

        AnsiConsole.Output.Write($"Classic Ansi Colors (8 colors)\n");
        foreach (AnsiColor8 bg in Enum.GetValues(typeof(AnsiColor8)))
        {
            foreach (AnsiColor8 fg in Enum.GetValues(typeof(AnsiColor8)))
            {
                AnsiConsole.Output.Write($"{Ansi.Color(fg, bg)}{fg}{Ansi.Reset} ");
            }
            AnsiConsole.Output.Write($"|\n");
        }
        AnsiConsole.Output.Write($"\n");

        AnsiConsole.Output.Write($"Aixterm Ansi Colors (16 colors)\n");
        foreach (AnsiColor16 bg in Enum.GetValues(typeof(AnsiColor16)))
        {
            foreach (AnsiColor16 fg in Enum.GetValues(typeof(AnsiColor16)))
            {
                AnsiConsole.Output.Write($"{Ansi.Color(fg, bg)}{fg}{Ansi.Reset} ");
            }
            AnsiConsole.Output.Write($"|\n");
        }
        AnsiConsole.Output.Write($"\n");

        AnsiConsole.Output.Write($"VGA Ansi Colors (256 colors)\n");
        //print only the 216 indexed rgb colors
        var colors = new Counter(0, 6 * 6 * 6).Select(i => (AnsiColor256)(16 + i)).ToList();
        foreach (var bg in colors)
        {
            foreach (var fg in colors)
            {
                AnsiConsole.Output.Write($"{Ansi.Color(fg, bg)}X");
            }
            AnsiConsole.Output.Write($"{Ansi.Reset}|\n");
        }
        AnsiConsole.Output.Write($"\n");
    }

    #endregion Private Methods
}
