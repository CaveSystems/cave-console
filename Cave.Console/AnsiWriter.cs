using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Cave.IO;
using Cave.Logging;

namespace Cave.Console;

/// <summary>Implements a writer for writing characters to a stream using ansi escape codes, encoding, specific newline and endianess.</summary>
public class AnsiWriter : IAnsiWriter, IDisposable
{
    #region Private Fields

    static readonly char[] CRLF = ['\r', '\n'];
    readonly Func<StringEncoding, NewLineMode, EndianType, DataWriter> getWriter;
    StringBuilder buffer = new();
    LogColor currentLogTextColor;
    bool disposedValue;

    #endregion Private Fields

    #region Private Methods

    DataWriter CreateWriter(StringEncoding encoding = 0, NewLineMode newLineMode = 0, EndianType endianType = 0)
    {
        if (Writer is not null)
        {
            if (encoding == 0) encoding = Writer.StringEncoding;
            if (newLineMode == 0) newLineMode = Writer.NewLineMode;
            if (endianType == 0) endianType = Writer.EndianType;
        }
        else
        {
            if (newLineMode == 0) newLineMode = Platform.IsMicrosoft ? NewLineMode.CRLF : NewLineMode.LF;
            if (endianType == 0) endianType = Endian.MachineType;
            if (encoding == 0) encoding = StringEncoding.UTF8;
        }
        return getWriter(encoding, newLineMode, endianType);
    }

    #endregion Private Methods

    #region Protected Properties

    /// <summary>Gets the underlying stream writer.</summary>
    protected DataWriter Writer { get; }

    #endregion Protected Properties

    #region Protected Methods

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing && IsStreamOwner)
            {
                Writer.BaseStream.Dispose();
            }

            disposedValue = true;
        }
    }

    #endregion Protected Methods

    #region Public Constructors

    /// <summary>Creates a new instance.</summary>
    /// <param name="stream">Stream to write to.</param>
    /// <param name="isStreamOwner">Controls whether the stream is freed if this instance is disposed or not.</param>
    public AnsiWriter(Stream stream, bool isStreamOwner) : this((StringEncoding encoding, NewLineMode newLineMode, EndianType endianType) => new DataWriter(stream, encoding, newLineMode, endianType)) => IsStreamOwner = isStreamOwner;

    /// <summary>Creates a new instance.</summary>
    /// <param name="getWriter"></param>
    public AnsiWriter(Func<StringEncoding, NewLineMode, EndianType, DataWriter> getWriter)
    {
        this.getWriter = getWriter;
        Writer = CreateWriter();
    }

    #endregion Public Constructors

    #region Public Properties

    /// <inheritdoc/>
    public CultureInfo CurrentCulture { get; set; } = Thread.CurrentThread.CurrentCulture;

    /// <summary>Gets or sets the endian encoder type.</summary>
    public EndianType EndianType { get => Writer.EndianType; set => Writer.EndianType = value; }

    /// <summary>Controls whether the stream is freed if this instance is disposed or not.</summary>
    public bool IsStreamOwner { get; }

    /// <summary>Gets or sets the new line mode used.</summary>
    public NewLineMode NewLineMode { get => Writer.NewLineMode; set => Writer.NewLineMode = value; }

    /// <summary>Sends an ansi reset after each newline.</summary>
    public bool ResetOnNewLine { get; set; } = true;

    /// <summary>Gets or sets encoding to use for characters and strings.</summary>
    public StringEncoding StringEncoding { get => Writer.StringEncoding; set => Writer.StringEncoding = value; }

    /// <summary>Gets or sets a value indicating whether the console waits until each line is completed.</summary>
    public bool WaitUntilNewLine { get; set; }

    #endregion Public Properties

    #region Public Methods

    /// <summary>Gets a writer for the teletypewriter interface with the specified <paramref name="device"/>.</summary>
    /// <param name="device">Path to device to open. E.g: /dev/tty1</param>
    /// <returns>Returns a writer for the specified device/file</returns>
    public static AnsiWriter Device(string device)
    {
        var tty = new FileStream(device, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, 1, FileOptions.WriteThrough);
        return new AnsiWriter(tty, true);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

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
            if (text.IndexOfAny(CRLF) < 0)
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

            var print = text[..(text.LastIndexOfAny(CRLF) + 1)];
            buffer.Append(text[print.Length..]);
            text = print;
        }

        var parts = text.SplitKeepSeparators(CRLF);
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
        WriteLine();
    }

    /// <inheritdoc/>
    public void WriteLine(ILogText text)
    {
        Write(text);
        WriteLine();
    }

    /// <inheritdoc/>
    public void WriteLine(IEnumerable<ILogText> items)
    {
        Write(items);
        WriteLine();
    }

    #endregion Public Methods
}
