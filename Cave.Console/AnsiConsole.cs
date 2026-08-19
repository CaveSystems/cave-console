using System;
using System.Collections.Generic;
using System.Globalization;
using Cave.Logging;

namespace Cave.Console;

/// <summary></summary>
public class AnsiConsole
{
    #region Private Fields

    static string title = string.Empty;

    #endregion Private Fields

    #region Private Methods

    static void InternalBackspace(ref string result)
    {
        if (result.Length > 0)
        {
            Output.Write("\b \b");
            result = result[..^1];
        }
    }

    static string InternalReadLine(char? replacementChar = null)
    {
        var reader = StandardInput.GetReader();
        var result = string.Empty;
        while (true)
        {
            var c = reader.ReadChar();
            switch (c)
            {
                case '\t': continue;
                case '\b':
                case '\u007f':
                {
                    InternalBackspace(ref result);
                    continue;
                }
                case '\r':
                case '\n': return result;
            }
            if (c == '\0' || char.IsControl(c))
            {
                continue;
            }
            result += c;
            Output.Write((replacementChar ?? c).ToString());
        }
    }

    #endregion Private Methods

    #region Public Properties

    /// <summary>Gets or sets the used culture to format strings.</summary>
    public static CultureInfo CurrentCulture { get => Output.CurrentCulture; set => Output.CurrentCulture = Error.CurrentCulture = value; }

    /// <summary>Writes to <see cref="StandardError"/></summary>
    /// <remarks>Functions of this writer are not locked. If you need multi thread access use <see cref="SyncRoot"/> for locking!</remarks>
    public static AnsiWriter Error { get; } = new AnsiWriter(StandardError.GetWriter);

    /// <summary>Writes to <see cref="StandardOutput"/></summary>
    /// <remarks>Functions of this writer are not locked. If you need multi thread access use <see cref="SyncRoot"/> for locking!</remarks>
    public static AnsiWriter Output { get; } = new AnsiWriter(StandardOutput.GetWriter);

    /// <summary>Multiuser sync root. This is used by all static functions of this class.</summary>
    public static object SyncRoot { get; } = new();

    /// <summary>Gets or sets the console title</summary>
    public static string Title
    {
        get => title;
        set
        {
            var newTitle = value ?? throw new ArgumentNullException(nameof(value));
            var ansi = new Ansi(newTitle, Output.StringEncoding);
            if (Platform.IsMicrosoft)
            {
                Write(Ansi.Control.Title(ansi));
            }
            else
            {
                Write(Ansi.Control.Title($"0;{ansi}"));
            }
            title = newTitle;
        }
    }

    #endregion Public Properties

    #region Public Methods

    /// <summary>Locks the console and reads an input line.</summary>
    /// <returns>Returns the string read.</returns>
    public static string ReadLine()
    {
        lock (SyncRoot)
        {
            return InternalReadLine();
        }
    }

    /// <summary>Locks the console and reads an input line showing only stars for each character.</summary>
    /// <returns>Returns the string read.</returns>
    public static string ReadPassword()
    {
        lock (SyncRoot)
        {
            return InternalReadLine('*');
        }
    }

    /// <summary>Clears the console.</summary>
    public static void Clear()
    {
        lock (SyncRoot)
        {
            Output.Write(Ansi.Reset + Ansi.ClearScreen);
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
    public static void Write(IEnumerable<ILogText> items)
    {
        lock (SyncRoot)
        {
            Output.Write(items);
        }
    }

    /// <inheritdoc/>
    public static void Write(IFormattable formattable)
    {
        lock (SyncRoot)
        {
            Output.Write(formattable);
        }
    }

    /// <inheritdoc/>
    public static void Write(ILogText item)
    {
        lock (SyncRoot)
        {
            Output.Write(item);
        }
    }

    /// <inheritdoc/>
    public static void Write(string text)
    {
        lock (SyncRoot)
        {
            Output.Write(text);
        }
    }

    #endregion Public Methods
}
