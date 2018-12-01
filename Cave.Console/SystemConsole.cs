using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cave.Collections.Generic;

namespace Cave.Console
{
    /// <summary>
    /// Provides access to the system console
    /// </summary>
    public static class SystemConsole
    {
        #region private implementation
        static XTColor m_CurrentColor = XTColor.Gray;
        static XTStyle m_CurrentStyle = XTStyle.Default;
        static bool m_Inverted = false;
        static bool m_WordWrap = true;
        static bool m_UseColor = true;
        static bool m_WaitUntilNewLine = false;
        static string m_Buffer = "";
        static string m_Title;
        static Queue<C<XTColor, XTStyle, bool>> m_ColorQueue = new Queue<C<XTColor, XTStyle, bool>>();

        static void InternalSetColors()
        {
            if (m_Inverted)
            {
                System.Console.ForegroundColor = ConsoleColor.Black;
                if (m_CurrentColor == XTColor.Default) { System.Console.BackgroundColor = ConsoleColor.Gray; }
                else { System.Console.BackgroundColor = XT.ToConsoleColor(m_CurrentColor); }
            }
            else
            {
                if (m_CurrentColor == XTColor.Default) { System.Console.ForegroundColor = ConsoleColor.Gray; }
                else { System.Console.ForegroundColor = XT.ToConsoleColor(m_CurrentColor); }
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
            m_CurrentColor = XTColor.Gray;
            m_Inverted = false;
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
            if (m_WaitUntilNewLine)
            {
                System.Console.WriteLine(m_Buffer);
                m_Buffer = "";
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

            m_CurrentColor = item.Color;
            m_CurrentStyle = item.Style;
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
            char[] CRLF = new char[] { '\r', '\n' };
            if (m_WaitUntilNewLine)
            {
                if (text.IndexOfAny(CRLF) < 0)
                {
                    m_Buffer += text;
                    return newLineCount;
                }
                if (m_Buffer.Length > 0)
                {
                    text = m_Buffer + text;
                }

                string print = text.Substring(0, text.LastIndexOfAny(CRLF) + 1);
                m_Buffer = text.Substring(print.Length);
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
                        if (m_WordWrap && (System.Console.CursorLeft + TabWidth >= System.Console.WindowWidth))
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
                if (m_WordWrap && (System.Console.CursorLeft + part.Length >= System.Console.WindowWidth))
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
            string result = "";
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
            string result = "";
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
        /// Creates a new SystemConsole instance
        /// </summary>
        static SystemConsole()
        {
            ReInitialize();
        }

        /// <summary>Initializes this instance.</summary>
        /// <exception cref="InvalidOperationException"></exception>
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
#elif NET35 || NET40 || NET45 || NET46 || NET471 || NETSTANDARD20
                System.Console.TreatControlCAsInput = true;
#else
#error No code defined for the current framework or NETXX version define missing!
#endif
                if (System.Console.KeyAvailable) { /* just check if we may call this */};
                CanReadKey = true;
            }
            catch { CanReadKey = false; }

            try { System.Console.Title = AssemblyVersionInfo.Program.ToString(); CanTitle = true; }
            catch { }

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
                        //disable word wrap on windows 10
                        Version winVer = Environment.OSVersion.Version;
                        if (LatestVersion.VersionIsNewer(winVer, new Version(6, 2)))
                        {
                            m_WordWrap = false;
                            //ClearEOL = true;
                        }
                    }
                    catch { }
                }
                if (System.Console.CursorLeft >= System.Console.WindowWidth)
                {
                    throw new InvalidOperationException();
                }

                CanWordWrap = CanPosition;
            }
            catch
            {
                m_WordWrap = false;
                CanWordWrap = false;
            }

            //console may be a stdout stream, wait until newline
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
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">
        /// Application has no console!
        /// or
        /// Input thread already started!
        /// </exception>
        public static void SetKeyPressedEvent(SystemConsoleKeyPressedDelegate keyPressedEvent)
        {
            lock (SyncRoot)
            {
                if (keyPressedEvent == null)
                {
                    throw new ArgumentNullException(nameof(keyPressedEvent));
                }

                if (!CanReadKey && IsConsoleAvailable)
                {
                    throw new Exception("Application has no console!");
                }

                if (inputThread != null)
                {
                    throw new Exception("Input thread already started!");
                }

                inputThread = new Thread(ConsoleReader);
                inputEvent = keyPressedEvent;
                inputThread.Start();
            }
        }

        /// <summary>Removes the key pressed event.</summary>
        /// <exception cref="InvalidOperationException">KeyPressedEvent was already removed!</exception>
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
        /// The number of leading spaces after a wordwrap
        /// </summary>
        public static int LeadingSpace = 2;

        /// <summary>Gets or sets a value indicating whether [clear eol shall be used on newline].</summary>
        /// <value><c>true</c> if [clear eol]; otherwise, <c>false</c>.</value>
        public static bool ClearEOL { get; set; }

        /// <summary>
        /// Enables / disables WordWrap
        /// </summary>
        public static bool WordWrap { get => m_WordWrap & CanWordWrap; set => m_WordWrap = value; }

        /// <summary>
        /// Enables / disables color
        /// </summary>
        public static bool UseColor { get => m_UseColor & CanColor; set => m_UseColor = value; }

        /// <summary>
        /// Waits until each line is completed
        /// </summary>
        public static bool WaitUntilNewLine { get => m_WaitUntilNewLine; set => m_WaitUntilNewLine = value; }

        /// <summary>
        /// Gets / sets the width of a tab character in spaces
        /// </summary>
        public static int TabWidth = 2;

        /// <summary>
        /// Obtains whether the console supports cursor positioning or not
        /// </summary>
        public static bool CanPosition { get; private set; }

        /// <summary>
        /// Obtains whether the console can print colors or not
        /// </summary>
        public static bool CanColor { get; private set; }

        /// <summary>
        /// Obtains whether the console can do word wrapping or not
        /// </summary>
        public static bool CanWordWrap { get; private set; }

        /// <summary>Gets a value indicating whether this instance can read key.</summary>
        /// <value>
        /// <c>true</c> if this instance can read key; otherwise, <c>false</c>.
        /// </value>
        public static bool CanReadKey { get; private set; }

        /// <summary>
        /// Obtains whether the console can do word wrapping or not
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
                    return (System.Console.BufferHeight != 0 || System.Console.BufferWidth != 0);
                }
                catch { return false; }
            }
        }

        /// <summary>Locks the console and reads a input line.</summary>
        /// <returns>Returns the string read</returns>
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
        /// <returns>Returns the string read</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", MessageId = "System.Console.Write(System.String)")]
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

        /// <summary>Ruft die nächste vom Benutzer gedrückte Zeichen- oder Funktionstaste ab.</summary>
        /// <returns></returns>
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
        /// <remarks>This returns false if no console windows is available or the output is redirected!</remarks>
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

                try { return System.Console.KeyAvailable; }
                catch { return false; }
            }
        }

        /// <summary>
        /// Gets/sets the current text color
        /// </summary>
        public static XTColor TextColor
        {
            get => m_CurrentColor;
            set => m_CurrentColor = value;
        }

        /// <summary>
        /// Gets/sets the current text color
        /// </summary>
        public static XTStyle TextStyle
        {
            get => m_CurrentStyle;
            set => m_CurrentStyle = value;
        }

        /// <summary>
        /// Invert the color (use color as background highlighter)
        /// </summary>
        public static bool Inverted
        {
            get => m_Inverted;
            set => m_Inverted = value;
        }

        /// <summary>
        /// Writes a string to the console (no formatting)
        /// </summary>
        /// <param name="text">The plain string to write</param>
        /// <returns>Returns the number of newlines printed</returns>
        public static int WriteString(string text)
        {
            lock (SyncRoot)
            {
                return InternalWriteString(text);
            }
        }

        /// <summary>
        /// Writes a LogText to the console (with formatting)
        /// </summary>
        /// <param name="text">The <see cref="XT"/> instance to write</param>
        /// <returns>Returns the number of newlines printed</returns>
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
        /// Writes a LogText to the console (with formatting)
        /// </summary>
        /// <param name="item">The <see cref="XT"/> instance to write</param>
        /// <returns>Returns the number of newlines printed</returns>
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
                if (!ReferenceEquals(text, null))
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
                if (!ReferenceEquals(text, null))
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
        /// Starts a new line at the console
        /// </summary>
        public static void NewLine()
        {
            lock (SyncRoot)
            {
                InternalNewLine();
            }
        }

        /// <summary>
        /// Resets the color to default value
        /// </summary>
        public static void ResetColor()
        {
            lock (SyncRoot)
            {
                InternalResetColor();
            }
        }

        /// <summary>
        /// Gets/sets the console title
        /// </summary>
        public static string Title
        {
            get { if (CanTitle) { lock (SyncRoot) { return System.Console.Title; } } return m_Title; }
            set
            {
                m_Title = value; if (CanTitle)
                {
                    lock (SyncRoot)
                    {
                        System.Console.Title = value;
                    }
                }
            }
        }

        /// <summary>
        /// Clears the console
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
        /// Clears everything till end of line
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
            C<XTColor, XTStyle, bool> i = m_ColorQueue.Dequeue();
            m_CurrentColor = i.V1;
            m_CurrentStyle = i.V2;
            m_Inverted = i.V3;
            if (CanColor)
            {
                InternalSetColors();
            }
        }

        /// <summary>Pushes the color to the stack.</summary>
        public static void PushColor()
        {
            m_ColorQueue.Enqueue(new C<XTColor, XTStyle, bool>(m_CurrentColor, m_CurrentStyle, m_Inverted));
        }
    }
}

