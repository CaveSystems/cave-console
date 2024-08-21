using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using Cave.Logging;

#pragma warning disable CS0618 // obsolete functions

namespace Cave.Console;

public class AnsiWriter : IAnsiWriter
{
    string buffer = string.Empty;
    LogColor currentLogTextColor;

    TextWriter Writer { get; }

    /// <inheritdoc/>
    public CultureInfo CurrentCulture { get; set; } = Thread.CurrentThread.CurrentCulture;

    /// <summary>Gets or sets a value indicating whether the console waits until each line is completed.</summary>
    public static bool WaitUntilNewLine { get; set; }

    public static string NewLine { get; set; } = Platform.Type switch { PlatformType.Windows or PlatformType.Xbox or PlatformType.CompactFramework => "\r\n", _ => "\n" };

    public static bool ResetOnNewLine { get; set; } = true;

    public AnsiWriter(TextWriter writer) => Writer = writer;

    /// <inheritdoc/>
    public void Write(Ansi ansi) => Writer.Write(ansi.ToString());

    /// <inheritdoc/>
    public int Write(IFormattable formattable)
    {
        var text = formattable.ToString(null, CurrentCulture);
        return Write(text);
    }

    /// <inheritdoc/>
    public int Write(ILogText item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (currentLogTextColor != item.Color)
        {
            currentLogTextColor = item.Color;
            switch (currentLogTextColor)
            {
                case LogColor.Black: Write(Ansi.FG.Black); break;
                case LogColor.Gray: Write(Ansi.FG.Gray); break;
                case LogColor.Blue: Write(Ansi.FG.Blue); break;
                case LogColor.Green: Write(Ansi.FG.Green); break;
                case LogColor.Cyan: Write(Ansi.FG.Cyan); break;
                case LogColor.Red: Write(Ansi.FG.Red); break;
                case LogColor.Magenta: Write(Ansi.FG.Magenta); break;
                case LogColor.Yellow: Write(Ansi.FG.Yellow); break;
                case LogColor.White: Write(Ansi.FG.White); break;
            }
        }
        switch (item.Style)
        {
            case LogStyle.Unchanged: break;
            case LogStyle.Reset: Write(Ansi.Reset); break;
            case LogStyle.Bold: Write(Ansi.Bold); break;
            case LogStyle.Italic: Write(Ansi.Italic); break;
            case LogStyle.Underline: Write(Ansi.Underline); break;
            case LogStyle.Inverse: Write(Ansi.Inverse); break;
        }

        return Write(item.Text);
    }

    /// <inheritdoc/>
    public int Write(IEnumerable<ILogText> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var newLineCount = 0;
        foreach (var item in items)
        {
            newLineCount += Write(item);
        }
        return newLineCount;
    }

    /// <inheritdoc/>
    public int Write(string text)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        var newLineCount = 0;
        if (WaitUntilNewLine)
        {
            if (text.IndexOfAny(['\r', '\n']) < 0)
            {
                buffer += text;
                return newLineCount;
            }
            if (buffer.Length > 0)
            {
                text = buffer + text;
            }

            var print = text[..(text.LastIndexOfAny(new char[] { '\r', '\n' }) + 1)];
            buffer = text[print.Length..];
            text = print;
        }

        var parts = text.SplitKeepSeparators('\r', '\n');
        var newline = false;
        foreach (var part in parts)
        {
            if (part == "\r")
            {
                newline = true;
                if (ResetOnNewLine) Writer.Write(Ansi.Reset);
                Writer.Write(NewLine);
                newLineCount++;
                continue;
            }
            if (part == "\n")
            {
                if (!newline)
                {
                    if (ResetOnNewLine) Writer.Write(Ansi.Reset);
                    Writer.Write(NewLine);
                    newLineCount++;
                }
                newline = false;
                continue;
            }
            newline = false;
            Writer.Write(part);
        }
        return newLineCount;
    }
}
