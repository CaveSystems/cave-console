using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Cave.IO;
using Cave.Logging;

#pragma warning disable CS0618 // obsolete functions

namespace Cave.Console;

public static class AnsiConsole
{
    static readonly object SyncRoot = new();
    static readonly AnsiWriter Output = new AnsiWriter(System.Console.Out);
    static readonly AnsiWriter Error = new AnsiWriter(System.Console.Error);

    public static CultureInfo CurrentCulture { get => Output.CurrentCulture; set => Output.CurrentCulture = Error.CurrentCulture = value; }

    public static string Title { get; set; }

    /// <summary>Clears the console.</summary>
    public static void Clear()
    {
        lock (SyncRoot)
        {
            Output.Write(Ansi.Clear());
        }
    }

    /// <inheritdoc/>
    public static void Write(Ansi ansi)
    {
        lock (SyncRoot)
        {
            Output.Write(ansi);
        }
    }

    /// <inheritdoc/>
    public static int Write(IEnumerable<ILogText> items)
    {
        lock (SyncRoot)
        {
            return Output.Write(items);
        }
    }

    /// <inheritdoc/>
    public static int Write(IFormattable formattable)
    {
        lock (SyncRoot)
        {
            return Output.Write(formattable);
        }
    }

    /// <inheritdoc/>
    public static int Write(ILogText item)
    {
        lock (SyncRoot)
        {
            return Output.Write(item);
        }
    }

    /// <inheritdoc/>
    public static int Write(string text)
    {
        lock (SyncRoot)
        {
            return Output.Write(text);
        }
    }
}
