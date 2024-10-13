using System;
using System.Collections.Generic;
using System.Globalization;
using Cave.Logging;

namespace Cave.Console;

/// <summary>Provides a writer interface</summary>
public interface IAnsiWriter
{
    #region Public Properties

    /// <summary>Gets or sets the culture used to format messages</summary>
    CultureInfo CurrentCulture { get; set; }

    #endregion Public Properties

    #region Public Methods

    /// <summary>Writes an ansi escape code</summary>
    /// <param name="ansi"></param>
    void Write(Ansi ansi);

    /// <summary>Writes the specified <paramref name="items"/></summary>
    /// <param name="items"></param>
    void Write(IEnumerable<ILogText> items);

    /// <summary>Formats and writes the <paramref name="formattable"/> parameter using the <see cref="CurrentCulture"/>.</summary>
    /// <param name="formattable"></param>
    void Write(IFormattable formattable);

    /// <summary>Formats and writes the <paramref name="item"/> parameter using the <see cref="CurrentCulture"/>.</summary>
    /// <param name="item"></param>
    void Write(ILogText item);

    /// <summary>Writes the specified <paramref name="text"/></summary>
    /// <param name="text"></param>
    void Write(string text);

    /// <summary>Writes the system dependent newline.</summary>
    void WriteLine();

    /// <summary>Writes the specified <paramref name="text"/> followed by the system dependent newline.</summary>
    /// <param name="text"></param>
    void WriteLine(string text);

    /// <summary>Writes the specified <paramref name="text"/> followed by the system dependent newline.</summary>
    /// <param name="text"></param>
    void WriteLine(ILogText text);

    /// <summary>Writes the specified <paramref name="items"/> followed by the system dependent newline.</summary>
    /// <param name="items"></param>
    void WriteLine(IEnumerable<ILogText> items);

    #endregion Public Methods
}
