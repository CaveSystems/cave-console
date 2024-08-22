using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Cave.IO;
using Cave.Logging;

namespace Cave.Console;

/// <summary>Implements a writer for writing characters to a stream using ansi escape codes, encoding, specific newline and endianess.</summary>
public class AnsiWriter : IAnsiWriter
{
    #region Private Fields

    StringBuilder buffer = new();
    LogColor currentLogTextColor;
    Func<StringEncoding, NewLineMode, EndianType, DataWriter> GetWriter;

    #endregion Private Fields

    #region Private Properties

    DataWriter Writer { get; set; }

    #endregion Private Properties

    #region Private Methods

    void UpdateWriter(StringEncoding encoding = 0, NewLineMode newLineMode = 0, EndianType endianType = 0)
    {
        if (Writer != null)
        {
            if (encoding == 0) encoding = Writer.StringEncoding;
            if (newLineMode == 0) newLineMode = Writer.NewLineMode;
            if (endianType == 0) endianType = Writer.EndianType;
        }
        Writer = GetWriter(encoding, newLineMode, endianType);
    }

    #endregion Private Methods

    #region Public Constructors

    /// <summary>Creates a new instance.</summary>
    /// <param name="stream"></param>
    public AnsiWriter(Stream stream) : this((StringEncoding encoding, NewLineMode newLineMode, EndianType endianType) => new DataWriter(stream, encoding, newLineMode, endianType)) { }

    /// <summary>Creates a new instance.</summary>
    /// <param name="getWriter"></param>
    public AnsiWriter(Func<StringEncoding, NewLineMode, EndianType, DataWriter> getWriter)
    {
        GetWriter = getWriter;
        UpdateWriter();
    }

    #endregion Public Constructors

    #region Public Properties

    /// <inheritdoc/>
    public CultureInfo CurrentCulture { get; set; } = Thread.CurrentThread.CurrentCulture;

    /// <summary>Gets or sets the endian encoder type.</summary>
    public EndianType EndianType { get => Writer.EndianType; set => UpdateWriter(endianType: value); }

    /// <summary>Gets or sets the new line mode used.</summary>
    public NewLineMode NewLineMode { get => Writer.NewLineMode; set => UpdateWriter(newLineMode: value); }

    /// <summary>Sends an ansi reset after each newline.</summary>
    public bool ResetOnNewLine { get; set; } = true;

    /// <summary>Gets or sets encoding to use for characters and strings.</summary>
    public StringEncoding StringEncoding { get => Writer.StringEncoding; set => UpdateWriter(encoding: value); }

    /// <summary>Gets or sets a value indicating whether the console waits until each line is completed.</summary>
    public bool WaitUntilNewLine { get; set; }

    #endregion Public Properties

    #region Public Methods

    /// <inheritdoc/>
    public void Write(Ansi ansi) => Writer.Write(ansi.Buffer);

    /// <inheritdoc/>
    public void Write(IFormattable formattable)
    {
        var text = formattable.ToString(null, CurrentCulture);
        Write(text);
    }

    /// <inheritdoc/>
    public void Write(ILogText item)
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

        Write(item.Text);
    }

    /// <inheritdoc/>
    public void Write(IEnumerable<ILogText> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        foreach (var item in items)
        {
            Write(item);
        }
    }

    /// <inheritdoc/>
    public void Write(string text)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (WaitUntilNewLine)
        {
            if (text.IndexOfAny(['\r', '\n']) < 0)
            {
                buffer.Append(text);
                return;
            }
            if (buffer.Length > 0)
            {
                buffer.Append(text);
                text = buffer.ToString();
                buffer = new();
            }

            var print = text[..(text.LastIndexOfAny(new char[] { '\r', '\n' }) + 1)];
            buffer.Append(text[print.Length..]);
            text = print;
        }

        var parts = text.SplitKeepSeparators('\r', '\n');
        var newline = false;
        foreach (var part in parts)
        {
            if (part == "\r")
            {
                newline = true;
                if (ResetOnNewLine) Write(Ansi.Reset);
                Writer.WriteLine();
                continue;
            }
            if (part == "\n")
            {
                if (!newline)
                {
                    if (ResetOnNewLine) Write(Ansi.Reset);
                    Writer.WriteLine();
                }
                newline = false;
                continue;
            }
            newline = false;
            Writer.Write(part);
        }
    }

    /// <inheritdoc/>
    public void WriteLine() => Writer.WriteLine();

    /// <inheritdoc/>
    public void WriteLine(string text)
    {
        Write(text);
        Writer.WriteLine();
    }

    #endregion Public Methods
}
