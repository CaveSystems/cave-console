using Cave.Collections;
using Cave.Console;

namespace SampleAnsiConsole;

class Program
{
    #region Private Methods

    static void Main(string[] args)
    {
        AnsiConsole.Write($"Classic Ansi Colors (8 colors)\n");
        foreach (AnsiColor8 bg in Enum.GetValues(typeof(AnsiColor8)))
        {
            foreach (AnsiColor8 fg in Enum.GetValues(typeof(AnsiColor8)))
            {
                AnsiConsole.Write($"{Ansi.Color(fg, bg)}{fg}{Ansi.Reset} ");
            }
            AnsiConsole.Write($"|\n");
        }
        AnsiConsole.Write($"\n");

        AnsiConsole.Write($"Aixterm Ansi Colors (16 colors)\n");
        foreach (AnsiColor16 bg in Enum.GetValues(typeof(AnsiColor16)))
        {
            foreach (AnsiColor16 fg in Enum.GetValues(typeof(AnsiColor16)))
            {
                AnsiConsole.Write($"{Ansi.Color(fg, bg)}{fg}{Ansi.Reset} ");
            }
            AnsiConsole.Write($"|\n");
        }
        AnsiConsole.Write($"\n");

        AnsiConsole.Write($"VGA Ansi Colors (256 colors)\n");
        //print only the 216 indexed rgb colors
        var colors = new Counter(0, 6 * 6 * 6).Select(i => (AnsiColor256)(16 + i)).ToList();
        foreach (var bg in colors)
        {
            foreach (var fg in colors)
            {
                AnsiConsole.Write($"{Ansi.Color(fg, bg)}X");
            }
            AnsiConsole.Write($"{Ansi.Reset}|\n");
        }
        AnsiConsole.Write($"\n");
    }

    #endregion Private Methods
}
