using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Cave;
using Cave.IO;
using Cave.Logging;

namespace Cave.Console;

/// <summary>Provides access to the system console.</summary>
public static class SystemConsole
{
    #region Private Classes

    class Item
    {
        #region Public Fields

        public LogColor Color;
        public bool Inverted;
        public LogStyle Style;

        #endregion Public Fields
    }

    #endregion Private Classes

    #region Private Fields

    static readonly Queue<Item> colorQueue = new();
    static readonly Queue<int> identQueue = new();
    static string buffer = string.Empty;
    static Thread inputThread;
    static string title;
    static bool useColor = true;
    static bool forceColor = false;
    static bool wordWrap = true;

    static event SystemConsoleKeyPressedDelegate inputEvent;

    #endregion Private Fields

    /// <summary>KeyPressed event available after a call to <see cref="StartKeyPressedMonitoring"/>.</summary>
    public static event SystemConsoleKeyPressedDelegate KeyPressed;

    #region Private Methods

    static void InternalKeyPressed(ConsoleKeyInfo keyInfo)
    {
        KeyPressed?.Invoke(keyInfo);
        inputEvent?.Invoke(keyInfo);
    }

    static void InternalClearToEOL() => InternalWriteString(new string(' ', System.Console.BufferWidth - System.Console.CursorLeft));

    /// <summary>(Re-)Initializes this instance.</summary>
    static void InternalInitialize()
    {
        var env = Environment.GetEnvironmentVariables();
        switch (env["color"])
        {
            case "always": forceColor = true; break;
            case "off":
            case "none": useColor = false; break;
        }

        try
        {
            IsConsoleAvailable = System.Console.BufferHeight != 0 || System.Console.BufferWidth != 0;
        }
        catch
        {
            IsConsoleAvailable = false;
        }

        if (!IsConsoleAvailable)
        {
            CanPosition = false;
            CanWordWrap = false;
            CanReadKey = false;
            WaitUntilNewLine = true;
            return;
        }

        try
        {
#if NET20
#else
            System.Console.TreatControlCAsInput = true;
#endif
            if (System.Console.KeyAvailable)
            {
                /* just check if we may call this */
            }
            CanReadKey = true;
        }
        catch
        {
            CanReadKey = false;
        }

        try
        {
            System.Console.Title = AssemblyVersionInfo.Program.ToString();
            CanTitle = true;
        }
        catch
        {
        }

        try
        {
            var saved = System.Console.CursorLeft;
            System.Console.CursorLeft = 1;
            if (System.Console.CursorLeft != 1)
            {
                CanPosition = false;
            }
            else
            {
                System.Console.CursorLeft = 0;
                CanPosition = System.Console.CursorLeft == 0;
            }
            System.Console.CursorLeft = saved;
        }
        catch
        {
            CanPosition = false;
        }

        try
        {
            DefaultForegroundColor = System.Console.ForegroundColor;
            DefaultBackgroundColor = System.Console.BackgroundColor;
            CanColor = CanPosition;
        }
        catch
        {
            CanColor = false;
        }
        try
        {
            if (Platform.IsMicrosoft)
            {
                try
                {
                    // disable word wrap on windows 10 and newer
                    var winVer = Environment.OSVersion.Version;
                    if (LatestVersion.VersionIsNewer(winVer, new Version(6, 2)))
                    {
                        wordWrap = false;
                    }
                }
                catch
                {
                }
            }
            if (System.Console.CursorLeft >= System.Console.WindowWidth)
            {
                throw new InvalidOperationException();
            }

            CanWordWrap = CanPosition;
        }
        catch
        {
            wordWrap = false;
            CanWordWrap = false;
        }

        // console may be a stdout stream, wait until newline
        if (!CanColor && !CanPosition && !CanWordWrap)
        {
            WaitUntilNewLine = true;
        }
    }

    static void InternalNewLine()
    {
        InternalResetColor();
        if (ClearEOL)
        {
            if (CanPosition)
            {
                var x = System.Console.CursorLeft;
                var y = System.Console.CursorTop;
                if (y == System.Console.BufferHeight - 1)
                {
                    y--;
                }

                InternalClearToEOL();
                System.Console.SetCursorPosition(x, y);
            }
            else
            {
                InternalClearToEOL();
                return;
            }
        }
        if (WaitUntilNewLine)
        {
            System.Console.WriteLine(buffer);
            buffer = string.Empty;
        }
        else
        {
            System.Console.WriteLine();
        }
    }

    static Logger log = new();

    static void InputEventThread()
    {
        Thread.CurrentThread.IsBackground = true;
        Thread.CurrentThread.Name = $"{nameof(SystemConsole)}.{nameof(InputEventThread)}";
        log.Debug($"{Thread.CurrentThread.Name} id {Thread.CurrentThread.ManagedThreadId} <green>start<reset>.");
        Exception lastException = null;
        var exceptionCounter = 0;
        while (inputThread != null)
        {
            try
            {
                if (System.Console.KeyAvailable)
                {
                    InternalKeyPressed(System.Console.ReadKey(true));
                    continue;
                }
                Thread.Sleep(1);
            }
            catch (Exception ex)
            {
                var wait = ++exceptionCounter;
                if (lastException != ex)
                {
                    lastException = ex;
                    log.Error(ex, $"Cannot read from console, retry in {((double)wait).FormatSeconds()}!");
                }
                Thread.Sleep(wait);
            }
        }
        log.Debug($"{Thread.CurrentThread.Name} id {Thread.CurrentThread.ManagedThreadId} <red>exit<reset>.");
    }

    static string InternalReadLine()
    {
        if (UseColor)
        {
            InternalSetColors();
        }

        var result = string.Empty;
        while (true)
        {
            var keyInfo = ReadKey();
            switch (keyInfo.Key)
            {
                case ConsoleKey.Backspace:
                {
                    if (result.Length > 0)
                    {
                        System.Console.Write(keyInfo.KeyChar + " " + keyInfo.KeyChar);
                        result = result[..^1];
                    }
                    continue;
                }
                case ConsoleKey.Enter: return result;
            }
            result += keyInfo.KeyChar;
            System.Console.Write(keyInfo.KeyChar);
        }
    }

    static string InternalReadPassword()
    {
        if (UseColor)
        {
            InternalSetColors();
        }

        var result = string.Empty;
        while (true)
        {
            var keyInfo = ReadKey();
            switch (keyInfo.Key)
            {
                case ConsoleKey.Tab:
                    continue;
                case ConsoleKey.Backspace:
                {
                    if (result.Length > 0)
                    {
                        System.Console.Write(keyInfo.KeyChar);
                        result = result[..^1];
                    }
                    continue;
                }
                case ConsoleKey.Enter: return result;
            }
            result += keyInfo.KeyChar;
            System.Console.Write("*");
        }
    }

    static void InternalResetColor()
    {
        TextColor = LogColor.Gray;
        Inverted = false;
        if (UseColor)
        {
            InternalSetColors();
        }
    }

    static void InternalSetColors()
    {
        var selectedColor = TextColor == 0 ? DefaultForegroundColor : LogText.ToConsoleColor(TextColor);
        if (Inverted)
        {
            System.Console.ForegroundColor = DefaultBackgroundColor;
            System.Console.BackgroundColor = selectedColor;
        }
        else
        {
            System.Console.ForegroundColor = selectedColor;
            System.Console.BackgroundColor = DefaultBackgroundColor;
        }
    }

    static int InternalWrite(ILogText item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        switch (item.Style)
        {
            case LogStyle.Unchanged: break;
            case LogStyle.Reset: InternalResetColor(); break;
            case LogStyle.Inverse: Inverted = true; break;
        }
        TextColor = item.Color == 0 ? TextColor : item.Color;

        if (UseColor)
        {
            InternalSetColors();
        }
        return InternalWriteString(item.Text);
    }

    static int InternalWrite(IEnumerable<ILogText> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var newLineCount = 0;
        foreach (var item in items)
        {
            newLineCount += InternalWrite(item);
        }
        return newLineCount;
    }

    static int InternalWriteString(string text)
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

            var print = text[..(text.LastIndexOfAny(['\r', '\n']) + 1)];
            buffer = text[print.Length..];
            if (UseColor || WordWrap)
            {
                text = print;
            }
            else
            {
                foreach (var line in print.SplitNewLine())
                {
                    var str = line.Replace("\t", new string(' ', Tabulator));
                    System.Console.WriteLine(str);
                    newLineCount++;
                }
                return newLineCount;
            }
        }

        if (UseColor)
        {
            InternalSetColors();
        }

        var parts = text.SplitKeepSeparators(' ', '\t', '\r', '\n');
        var newline = false;
        foreach (var part in parts)
        {
            if (part == "\r")
            {
                newline = true;
                InternalNewLine();
                newLineCount++;
                continue;
            }
            if (part == "\n")
            {
                if (!newline)
                {
                    InternalNewLine();
                    newLineCount++;
                }
                newline = false;
                continue;
            }
            newline = false;
            if (part == "\t")
            {
                if (CanPosition)
                {
                    if (wordWrap && (System.Console.CursorLeft + Tabulator >= System.Console.WindowWidth))
                    {
                        if (UseColor)
                        {
                            PushColor();
                        }

                        InternalNewLine();
                        newLineCount++;
                        System.Console.Write(new string(' ', Ident));
                        if (UseColor)
                        {
                            PopColor();
                        }
                    }
                    else
                    {
                        System.Console.Write(new string(' ', Tabulator));
                    }
                }
                continue;
            }
            if (wordWrap && (System.Console.CursorLeft + part.Length >= System.Console.WindowWidth))
            {
                if (UseColor)
                {
                    PushColor();
                }

                InternalNewLine();
                System.Console.Write(new string(' ', Ident));
                if (UseColor)
                {
                    PopColor();
                }
            }
            System.Console.Write(part);
        }
        return newLineCount;
    }

    #endregion Private Methods

    #region Public Constructors

    /// <summary>Initializes static members of the <see cref="SystemConsole"/> class.</summary>
    static SystemConsole()
    {
        Codepage.Init();
        ReInitialize();
    }

    #endregion Public Constructors

    #region Public Properties

    /// <summary>Gets a value indicating whether the console can print colors or not.</summary>
    public static bool CanColor { get; private set; }

    /// <summary>Gets a value indicating whether the console supports cursor positioning or not.</summary>
    public static bool CanPosition { get; private set; }

    /// <summary>Gets a value indicating whether this instance can read key.</summary>
    /// <value><c>true</c> if this instance can read key; otherwise, <c>false</c>.</value>
    public static bool CanReadKey { get; private set; }

    /// <summary>Gets a value indicating whether the console can do word wrapping or not.</summary>
    public static bool CanTitle { get; private set; }

    /// <summary>Gets a value indicating whether the console can do word wrapping or not.</summary>
    public static bool CanWordWrap { get; private set; }

    /// <summary>Gets or sets a value indicating whether [clear eol shall be used on newline].</summary>
    /// <value><c>true</c> if [clear eol]; otherwise, <c>false</c>.</value>
    public static bool ClearEOL { get; set; }

    /// <summary>The number of leading spaces after a wordwrap.</summary>
    public static int Ident { get; set; } = 2;

    /// <summary>Gets or sets a value indicating whether the color shall be inverted (use color as background highlighter).</summary>
    public static bool Inverted { get; set; }

    /// <summary>Gets a value indicating whether a console is available.</summary>
    public static bool IsConsoleAvailable { get; private set; }

    /// <summary>Gets a value indicating whether [key is available].</summary>
    /// <remarks>This returns false if no console windows is available or the output is redirected!.</remarks>
    /// <value><c>true</c> if [key available]; otherwise, <c>false</c>.</value>
    public static bool KeyAvailable
    {
        get
        {
            if (inputThread != null)
            {
                throw new InvalidOperationException("An input reader was already connected to the SystemConsole!");
            }

            if (!CanReadKey)
            {
                return false;
            }

            try
            {
                return System.Console.KeyAvailable;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>Gets the synchronize root.</summary>
    /// <value>The synchronize root.</value>
    public static object SyncRoot { get; } = new object();

    /// <summary>Gets / sets the width of a tab character in spaces.</summary>
    public static int Tabulator { get; set; } = 2;

    /// <summary>Gets or sets the current text color.</summary>
    public static LogColor TextColor { get; set; } = LogColor.Gray;

    /// <summary>Gets or sets the current text color.</summary>
    public static LogStyle TextStyle { get; set; } = LogStyle.Unchanged;

    /// <summary>Gets or sets the console title.</summary>
    public static string Title
    {
        [SuppressMessage("Usage", "CA1416")]
        get
        {
            if (CanTitle)
            {
                lock (SyncRoot)
                {
                    title = System.Console.Title;
                }
            }
            return title;
        }

        set
        {
            title = value;
            if (CanTitle)
            {
                System.Console.Title = value;
            }
        }
    }

    /// <summary>Gets or sets a value indicating whether color is enabled or not.</summary>
    public static bool UseColor { get => useColor && (CanColor || forceColor); set => useColor = value; }

    /// <summary>Gets or sets a value indicating whether the console waits until each line is completed.</summary>
    public static bool WaitUntilNewLine { get; set; }

    /// <summary>Gets or sets a value indicating whether WordWrap is enabled or not.</summary>
    public static bool WordWrap { get => wordWrap && CanWordWrap; set => wordWrap = value; }

    /// <summary>Gets or sets the default forground color.</summary>
    public static ConsoleColor DefaultForegroundColor { get; set; }

    /// <summary>Gets or sets the default background color.</summary>
    public static ConsoleColor DefaultBackgroundColor { get; set; }

    #endregion Public Properties

    #region Public Methods

    /// <summary>Clears the console.</summary>
    public static void Clear()
    {
        if (IsConsoleAvailable)
        {
            lock (SyncRoot)
            {
                System.Console.Clear();
            }
        }
    }

    /// <summary>Clears everything till end of line.</summary>
    public static void ClearToEOL()
    {
        if (IsConsoleAvailable)
        {
            lock (SyncRoot)
            {
                InternalClearToEOL();
            }
        }
    }

    /// <summary>Starts a new line at the console.</summary>
    public static void NewLine()
    {
        lock (SyncRoot)
        {
            InternalNewLine();
        }
    }

    /// <summary>Pops the color from the stack.</summary>
    public static void PopColor()
    {
        lock (SyncRoot)
        {
            var i = colorQueue.Dequeue();
            TextColor = i.Color;
            TextStyle = i.Style;
            Inverted = i.Inverted;
            if (CanColor)
            {
                InternalSetColors();
            }
        }
    }

    /// <summary>Pops the color from the stack.</summary>
    public static void PopIdent()
    {
        lock (SyncRoot)
        {
            Ident = identQueue.Dequeue();
        }
    }

    /// <summary>Pushes the color to the stack.</summary>
    public static void PushColor()
    {
        lock (SyncRoot)
        {
            colorQueue.Enqueue(new Item() { Color = TextColor, Style = TextStyle, Inverted = Inverted });
        }
    }

    /// <summary>Pushes the color to the stack.</summary>
    public static void PushIdent()
    {
        lock (SyncRoot)
        {
            identQueue.Enqueue(Ident);
        }
    }

    /// <summary>Gets the next character or function key. This will block until a key is available.</summary>
    /// <returns>Returns the next readable console key.</returns>
    public static ConsoleKeyInfo ReadKey()
    {
        while (inputThread != null)
        {
            var done = new ManualResetEvent(false);
            ConsoleKeyInfo? result = null;
            KeyPressed += (c) =>
            {
                result = c;
                done.Set();
            };
            done.WaitOne();
            if (result.HasValue) return result.Value;
        }

        if (!CanReadKey)
        {
            throw new Exception("No console available, read input stream!");
        }

        lock (SyncRoot)
        {
            return System.Console.ReadKey(true);
        }
    }

    /// <summary>Locks the console and reads a input line.</summary>
    /// <returns>Returns the string read.</returns>
    public static string ReadLine()
    {
        lock (SyncRoot)
        {
            return InternalReadLine();
        }
    }

    /// <summary>Locks the console and reads a input line showing only stars for each character.</summary>
    /// <returns>Returns the string read.</returns>
    public static string ReadPassword()
    {
        lock (SyncRoot)
        {
            return InternalReadPassword();
        }
    }

    /// <summary>(Re-)Initializes this instance.</summary>
    public static void ReInitialize()
    {
        lock (SyncRoot)
        {
            InternalInitialize();
        }
    }

    /// <summary>Resets the color to default value.</summary>
    public static void ResetColor()
    {
        lock (SyncRoot)
        {
            InternalResetColor();
        }
    }

    /// <summary>Sets the default foreground to <see cref="ConsoleColor.Gray"/> and the background to <see cref="ConsoleColor.Black"/>.</summary>
    public static void SetDefaultColors()
    {
        lock (SyncRoot)
        {
            DefaultForegroundColor = ConsoleColor.Gray;
            DefaultBackgroundColor = ConsoleColor.Black;
        }
    }

    /// <summary>Starts the key pressed monitoring.</summary>
    /// <remarks>Use the <see cref="KeyPressed"/> event for receifing pressed keys.</remarks>
    /// <exception cref="ArgumentNullException">KeyPressedEvent is null.</exception>
    /// <exception cref="InvalidOperationException">Application has no console! or Input thread already started!.</exception>
    public static void StartKeyPressedMonitoring()
    {
        lock (SyncRoot)
        {
            if (inputThread?.IsAlive == true) return;
            if (!CanReadKey && IsConsoleAvailable)
            {
                throw new InvalidOperationException("Application has no console!");
            }

            inputThread = new Thread(InputEventThread);
            inputThread.Start();
        }
    }

    /// <summary>Sets the key pressed event.</summary>
    /// <param name="keyPressedEvent">The key pressed event.</param>
    /// <exception cref="ArgumentNullException">KeyPressedEvent is null.</exception>
    /// <exception cref="InvalidOperationException">Application has no console! or Input thread already started!.</exception>
    [Obsolete($"Use {nameof(KeyPressed)} event and {nameof(StartKeyPressedMonitoring)}")]
    public static void SetKeyPressedEvent(SystemConsoleKeyPressedDelegate keyPressedEvent)
    {
        lock (SyncRoot)
        {
            inputEvent = keyPressedEvent ?? throw new ArgumentNullException(nameof(keyPressedEvent));
            StartKeyPressedMonitoring();
        }
    }

    /// <summary>Removes the key pressed event.</summary>
    /// <exception cref="InvalidOperationException">KeyPressedEvent was already removed!.</exception>
    [Obsolete($"Use {nameof(KeyPressed)} event and {nameof(StartKeyPressedMonitoring)}")]
    public static void RemoveKeyPressedEvent()
    {
        lock (SyncRoot)
        {
            if (inputEvent is null) throw new InvalidOperationException("KeyPressedEvent was already removed!");
            inputEvent = null;
        }
    }

    /// <summary>Writes a LogText to the console (with formatting).</summary>
    /// <param name="text">The content to write.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>Returns the number of newlines printed.</returns>
    public static int Write(string text, params object[] args)
    {
        var items = LogText.Parse(string.Format(text, args));
        lock (SyncRoot)
        {
            items = items.Select(i => i.Color == 0 ? new LogText(i.Text, TextColor) : i).ToList();
            return InternalWrite(items);
        }
    }

    /// <summary>Writes a LogText to the console (with formatting).</summary>
    /// <param name="text">The text to write.</param>
    /// <returns>Returns the number of newlines printed.</returns>
    public static int Write(string text)
    {
        var items = LogText.Parse(text);
        lock (SyncRoot)
        {
            return InternalWrite(items);
        }
    }

    /// <summary>Writes a LogText to the console (with formatting).</summary>
    /// <param name="text">The text.</param>
    /// <returns>Returns the number of newlines printed.</returns>
    public static int Write(ILogText text)
    {
        lock (SyncRoot)
        {
            return InternalWrite(text);
        }
    }

    /// <summary>Writes the specified <paramref name="items"/> followed by newline.</summary>
    /// <param name="items">The items.</param>
    /// <returns>Returns the number of newlines printed.</returns>
    public static int WriteLine(IEnumerable<ILogText> items = null)
    {
        lock (SyncRoot)
        {
            var i = 0;
            if (items is not null)
            {
                i += InternalWrite(items);
            }

            InternalNewLine();
            return ++i;
        }
    }

    /// <summary>Writes the specified <paramref name="items"/>.</summary>
    /// <param name="items">The items.</param>
    /// <returns>Returns the number of newlines printed.</returns>
    public static int Write(IEnumerable<ILogText> items = null)
    {
        lock (SyncRoot)
        {
            var i = 0;
            if (items is not null)
            {
                i += InternalWrite(items);
            }

            return i;
        }
    }

    /// <summary>Writes the line.</summary>
    /// <param name="text">The text.</param>
    public static int WriteLine(ILogText text)
    {
        lock (SyncRoot)
        {
            var i = 0;
            if (text is not null)
            {
                i += InternalWrite(text);
            }

            InternalNewLine();
            return ++i;
        }
    }

    /// <summary>Writes the line.</summary>
    /// <param name="text">The text.</param>
    /// <param name="args">The arguments.</param>
    public static int WriteLine(string text, params object[] args) => WriteLine(LogText.Parse(string.Format(text, args)));

    /// <summary>Writes a string to the console (no formatting).</summary>
    /// <param name="text">The plain string to write.</param>
    /// <returns>Returns the number of newlines printed.</returns>
    public static int WriteString(string text)
    {
        lock (SyncRoot)
        {
            return InternalWriteString(text);
        }
    }

    #endregion Public Methods
}
