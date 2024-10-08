using System;
using System.Collections.Generic;
using System.Globalization;
using Cave.Logging;

namespace Cave.Console;

/// <summary></summary>
public static class AnsiConsole
{
    #region Private Fields

    static string title = string.Empty;

    #endregion Private Fields

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
