using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cave.Collections.Generic;

namespace Cave.Console
{
    /// <summary>
    /// Provides access to the system console.
    /// </summary>
    public static class SystemConsole
    {
        #region private implementation
        static readonly Queue<Item> colorQueue = new Queue<Item>();
        static bool wordWrap = true;
        static bool useColor = true;
        static string buffer = string.Empty;
        static string title;

        static void InternalSetColors()
        {
            ConsoleColor selectedColor = TextColor == XTColor.Default ? ConsoleColor.Gray : XT.ToConsoleColor(TextColor);
            if (Inverted)
            {
                System.Console.ForegroundColor = ConsoleColor.Black;
                System.Console.BackgroundColor = selectedColor;
            }
            else
            {
                System.Console.ForegroundColor = selectedColor;
                System.Console.BackgroundColor = ConsoleColor.Black;
            }
        }

        static void InternalResetColor()
        {
            if (UseColor)
            {
                System.Console.ForegroundColor = ConsoleColor.Gray;
                System.Console.BackgroundColor = ConsoleColor.Black;
            }
            TextColor = XTColor.Gray;
            Inverted = false;
        }

        static void InternalNewLine()
        {
            InternalResetColor();
            if (ClearEOL)
            {
                if (CanPosition)
                {
                    int x = System.Console.CursorLeft;
                    int y = System.Console.CursorTop;
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
                System.Console.Write("\r\n");
            }
        }

        static int InternalWrite(XTItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            TextColor = item.Color;
            TextStyle = item.Style;
            return InternalWriteString(item.Text);
        }

        static int InternalWrite(XT text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            int newLineCount = 0;
            foreach (XTItem item in text.Items)
            {
                newLineCount += InternalWrite(item);
            }
            return newLineCount;
        }

        static int InternalWriteString(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            int newLineCount = 0;
            if (WaitUntilNewLine)
            {
                if (text.IndexOfAny(new char[] { '\r', '\n' }) < 0)
                {
                    buffer += text;
                    return newLineCount;
                }
                if (buffer.Length > 0)
                {
                    text = buffer + text;
                }

                string print = text.Substring(0, text.LastIndexOfAny(new char[] { '\r', '\n' }) + 1);
                buffer = text.Substring(print.Length);
                if (UseColor || WordWrap)
                {
                    text = print;
                }
                else
                {
                    #region print buffer without word wrapping and color
                    foreach (string line in print.Split(new string[] { "\r\n" }, StringSplitOptions.None))
                    {
                        string str = line.Replace("\t", new string(' ', TabWidth));
                        System.Console.WriteLine(str);
                        newLineCount++;
                    }
                    return newLineCount;
                    #endregion
                }
            }

            if (UseColor)
            {
                InternalSetColors();
            }

            string[] parts = text.SplitKeepSeparators(' ', '\t', '\r', '\n');
            bool newline = false;
            foreach (string part in parts)
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
                        if (wordWrap && (System.Console.CursorLeft + TabWidth >= System.Console.WindowWidth))
                        {
                            if (UseColor)
                            {
                                PushColor();
                            }

                            InternalNewLine();
                            newLineCount++;
                            System.Console.Write(new string(' ', LeadingSpace));
                            if (UseColor)
                            {
                                PopColor();
                            }
                        }
                        else
                        {
                            System.Console.Write(new string(' ', TabWidth));
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
                    System.Console.Write(new string(' ', LeadingSpace));
                    if (UseColor)
                    {
                        PopColor();
                    }
                }
                System.Console.Write(part);
            }
            return newLineCount;
        }

        static string InternalReadLine()
        {
            string result = string.Empty;
            while (true)
            {
                ConsoleKeyInfo keyInfo = System.Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.Backspace:
                    {
                        if (result.Length > 0)
                        {
                            System.Console.Write(keyInfo.KeyChar + " " + keyInfo.KeyChar);
                            result = result.Substring(0, result.Length - 1);
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
            string result = string.Empty;
            while (true)
            {
                ConsoleKeyInfo keyInfo = System.Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.Backspace:
                    {
                        if (result.Length > 0)
                        {
                            System.Console.Write(keyInfo.KeyChar);
                            result = result.Substring(0, result.Length - 1);
                        }
                        continue;
                    }
                    case ConsoleKey.Enter: return result;
                }
                result += keyInfo.KeyChar;
                System.Console.Write("*");
            }
        }

        static void InternalClearToEOL()
        {
            InternalWriteString(new string(' ', System.Console.BufferWidth - System.Console.CursorLeft));
        }

        /// <summary>
        /// Initializes static members of the <see cref="SystemConsole"/> class.
        /// </summary>
        static SystemConsole()
        {
            ReInitialize();
        }

        /// <summary>(Re-)Initializes this instance.</summary>
        public static void ReInitialize()
        {
            if (!IsConsoleAvailable)
            {
                CanColor = false;
                CanPosition = false;
                CanWordWrap = false;
                CanReadKey = false;
                WaitUntilNewLine = true;
                return;
            }

            try
            {
#if NET20
#elif NET35 || NET40 || NET45 || NET46 || NET47 || NETSTANDARD20
                System.Console.TreatControlCAsInput = true;
#else
#error No code defined for the current framework or NETXX version define missing!
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
            }
            catch
            {
                CanPosition = false;
            }

            try
            {
                System.Console.ForegroundColor = ConsoleColor.Gray;
                System.Console.BackgroundColor = ConsoleColor.Black;
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
                        // disable word wrap on windows 10
                        Version winVer = Environment.OSVersion.Version;
                        if (LatestVersion.VersionIsNewer(winVer, new Version(6, 2)))
                        {
                            wordWrap = false;

                            // ClearEOL = true;
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

        #endregion

        #region public implementation

        static Thread inputThread;
        static SystemConsoleKeyPressedDelegate inputEvent;
        static void ConsoleReader()
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "SystemConsole.Reader";
            Trace.TraceInformation("System Console Input Event Thread start.");
            while (inputThread != null)
            {
                try
                {
                    if (System.Console.KeyAvailable)
                    {
                        inputEvent(System.Console.ReadKey(true));
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Cannot read from console!\n{0}", ex);
                }
                Thread.Sleep(250);
            }
            Trace.TraceInformation("System Console Input Event Thread exit.");
        }

        /// <summary>Sets the key pressed event.</summary>
        /// <param name="keyPressedEvent">The key pressed event.</param>
        /// <exception cref="ArgumentNullException">KeyPressedEvent is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Application has no console!
        /// or
        /// Input thread already started!.
        /// </exception>
        public static void SetKeyPressedEvent(SystemConsoleKeyPressedDelegate keyPressedEvent)
        {
            lock (SyncRoot)
            {
                if (!CanReadKey && IsConsoleAvailable)
                {
                    throw new InvalidOperationException("Application has no console!");
                }

                if (inputThread != null)
                {
                    throw new InvalidOperationException("Input thread already started!");
                }

                inputEvent = keyPressedEvent ?? throw new ArgumentNullException(nameof(keyPressedEvent));
                inputThread = new Thread(ConsoleReader);
                inputThread.Start();
            }
        }

        /// <summary>Removes the key pressed event.</summary>
        /// <exception cref="InvalidOperationException">KeyPressedEvent was already removed!.</exception>
        public static void RemoveKeyPressedEvent()
        {
            lock (SyncRoot)
            {
                Thread t = inputThread;
                inputThread = null;
                while (t != null && t.IsAlive)
                {
                    Thread.Sleep(1);
                }
            }
        }

        /// <summary>Gets the synchronize root.</summary>
        /// <value>The synchronize root.</value>
        public static object SyncRoot { get; } = new object();

        /// <summary>
        /// The number of leading spaces after a wordwrap.
        /// </summary>
        public static int LeadingSpace = 2;

        /// <summary>Gets or sets a value indicating whether [clear eol shall be used on newline].</summary>
        /// <value><c>true</c> if [clear eol]; otherwise, <c>false</c>.</value>
        public static bool ClearEOL { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether WordWrap is enabled or not.
        /// </summary>
        public static bool WordWrap { get => wordWrap & CanWordWrap; set => wordWrap = value; }

        /// <summary>
        /// Gets or sets a value indicating whether color is enabled or not.
        /// </summary>
        public static bool UseColor { get => useColor & CanColor; set => useColor = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the console waits until each line is completed.
        /// </summary>
        public static bool WaitUntilNewLine { get; set; } = false;

        /// <summary>
        /// Gets / sets the width of a tab character in spaces.
        /// </summary>
        public static int TabWidth = 2;

        /// <summary>
        /// Gets a value indicating whether the console supports cursor positioning or not.
        /// </summary>
        public static bool CanPosition { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the console can print colors or not.
        /// </summary>
        public static bool CanColor { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the console can do word wrapping or not.
        /// </summary>
        public static bool CanWordWrap { get; private set; }

        /// <summary>Gets a value indicating whether this instance can read key.</summary>
        /// <value>
        /// <c>true</c> if this instance can read key; otherwise, <c>false</c>.
        /// </value>
        public static bool CanReadKey { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the console can do word wrapping or not.
        /// </summary>
        public static bool CanTitle { get; private set; }

        /// <summary>Gets a value indicating whether a console is available.</summary>
        public static bool IsConsoleAvailable
        {
            get
            {
                try
                {
                    bool b = System.Console.KeyAvailable;
                    return System.Console.BufferHeight != 0 || System.Console.BufferWidth != 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>Locks the console and reads a input line.</summary>
        /// <returns>Returns the string read.</returns>
        public static string ReadLine()
        {
            if (inputThread != null)
            {
                throw new InvalidOperationException("An input reader was already connected to the SystemConsole!");
            }

            lock (SyncRoot)
            {
                return InternalReadLine();
            }
        }

        /// <summary>Locks the console and reads a input line showing only stars for each character.</summary>
        /// <returns>Returns the string read.</returns>
        public static string ReadPassword()
        {
            if (inputThread != null)
            {
                throw new InvalidOperationException("An input reader was already connected to the SystemConsole!");
            }

            lock (SyncRoot)
            {
                return InternalReadPassword();
            }
        }

        /// <summary>Gets the next character or function key. This will block until a key is available.</summary>
        /// <returns>Returns the next readable console key.</returns>
        public static ConsoleKeyInfo ReadKey()
        {
            if (inputThread != null)
            {
                throw new InvalidOperationException("An input reader was already connected to the SystemConsole!");
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

        /// <summary>
        /// Gets or sets the current text color.
        /// </summary>
        public static XTColor TextColor { get; set; } = XTColor.Gray;

        /// <summary>
        /// Gets or sets the current text color.
        /// </summary>
        public static XTStyle TextStyle { get; set; } = XTStyle.Default;

        /// <summary>
        /// Gets or sets a value indicating whether the color shall be inverted (use color as background highlighter).
        /// </summary>
        public static bool Inverted { get; set; } = false;

        /// <summary>
        /// Writes a string to the console (no formatting).
        /// </summary>
        /// <param name="text">The plain string to write.</param>
        /// <returns>Returns the number of newlines printed.</returns>
        public static int WriteString(string text)
        {
            lock (SyncRoot)
            {
                return InternalWriteString(text);
            }
        }

        /// <summary>
        /// Writes a LogText to the console (with formatting).
        /// </summary>
        /// <param name="text">The <see cref="XT"/> instance to write.</param>
        /// <returns>Returns the number of newlines printed.</returns>
        public static int Write(XT text)
        {
            lock (SyncRoot)
            {
                return InternalWrite(text);
            }
        }

        /// <summary>Writes the line.</summary>
        /// <param name="text">The text.</param>
        /// <param name="args">The arguments.</param>
        public static void Write(XT text, params object[] args)
        {
            lock (SyncRoot)
            {
                InternalWrite(XT.Format(text, args));
            }
        }

        /// <summary>
        /// Writes a LogText to the console (with formatting).
        /// </summary>
        /// <param name="item">The <see cref="XT"/> instance to write.</param>
        /// <returns>Returns the number of newlines printed.</returns>
        public static int Write(XTItem item)
        {
            lock (SyncRoot)
            {
                return InternalWrite(item);
            }
        }

        /// <summary>Writes the line.</summary>
        /// <param name="text">The text.</param>
        public static void WriteLine(XT text = null)
        {
            lock (SyncRoot)
            {
                if (text is object)
                {
                    InternalWrite(text);
                }

                InternalNewLine();
            }
        }

        /// <summary>Writes the line.</summary>
        /// <param name="text">The text.</param>
        public static void WriteLine(IXT text)
        {
            lock (SyncRoot)
            {
                if (text is object)
                {
                    InternalWrite(text.ToXT());
                }

                InternalNewLine();
            }
        }

        /// <summary>Writes the line.</summary>
        /// <param name="text">The text.</param>
        /// <param name="args">The arguments.</param>
        public static void WriteLine(string text, params object[] args)
        {
            lock (SyncRoot)
            {
                InternalWrite(XT.Format(text, args));
                InternalNewLine();
            }
        }

        /// <summary>Writes the line.</summary>
        /// <param name="item">The item.</param>
        public static void WriteLine(XTItem item)
        {
            lock (SyncRoot)
            {
                InternalWrite(item);
                InternalNewLine();
            }
        }

        /// <summary>
        /// Starts a new line at the console.
        /// </summary>
        public static void NewLine()
        {
            lock (SyncRoot)
            {
                InternalNewLine();
            }
        }

        /// <summary>
        /// Resets the color to default value.
        /// </summary>
        public static void ResetColor()
        {
            lock (SyncRoot)
            {
                InternalResetColor();
            }
        }

        /// <summary>
        /// Gets or sets the console title.
        /// </summary>
        public static string Title
        {
            get
            {
                if (CanTitle)
                {
                    lock (SyncRoot)
                    {
                        return System.Console.Title;
                    }
                }
                return title;
            }

            set
            {
                title = value;
                if (CanTitle)
                {
                    lock (SyncRoot)
                    {
                        System.Console.Title = value;
                    }
                }
            }
        }

        /// <summary>
        /// Clears the console.
        /// </summary>
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

        /// <summary>
        /// Clears everything till end of line.
        /// </summary>
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

        #endregion

        /// <summary>Pops the color from the stack.</summary>
        public static void PopColor()
        {
            Item i = colorQueue.Dequeue();
            TextColor = i.Color;
            TextStyle = i.Style;
            Inverted = i.Inverted;
            if (CanColor)
            {
                InternalSetColors();
            }
        }

        /// <summary>Pushes the color to the stack.</summary>
        public static void PushColor()
        {
            colorQueue.Enqueue(new Item() { Color = TextColor, Style = TextStyle, Inverted = Inverted });
        }

        class Item
        {
            public XTColor Color;
            public XTStyle Style;
            public bool Inverted;
        }
    }
}
