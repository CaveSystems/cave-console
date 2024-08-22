#pragma warning disable CS0618 // obsolete functions

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Cave.IO;

namespace Cave.Console;

/// <summary>Ansi control codes for terminal escape codes</summary>
public class Ansi : IEnumerable<byte>
{
    /// <summary>Creates a new ansi instance containing the specified data.</summary>
    /// <param name="ansiString">String to encode</param>
    /// <param name="encoding">Encoding to use</param>
    protected internal Ansi(string ansiString, StringEncoding encoding = StringEncoding.ASCII) : this(encoding.Encode(ansiString)) { }

    /// <summary>Creates a new ansi instance containing the specified data</summary>
    /// <param name="buffer"></param>
    internal Ansi(byte[] buffer)
    {
        Debug.Assert(buffer.All(b => b < 128));
        Buffer = buffer;
    }

    /// <summary>Converts a (ascii) string to a new instance.</summary>
    /// <param name="ansiString">String to convert</param>
    public static implicit operator Ansi(string ansiString) => new(ASCII.GetBytes(ansiString));

    /// <summary>Combines to ansi escape strings.</summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static Ansi operator +(Ansi left, Ansi right)
    {
        var result = new byte[left.Buffer.Length + right.Buffer.Length];
        System.Buffer.BlockCopy(left.Buffer, 0, result, 0, left.Buffer.Length);
        System.Buffer.BlockCopy(right.Buffer, 0, result, left.Buffer.Length, right.Buffer.Length);
        return new(result);
    }

    /// <summary>Gets the ansi byte buffer.</summary>
    protected internal byte[] Buffer { get; }

    /// <summary>Gets the length of the ansi escape string.</summary>
    public int Length => Buffer.Length;

    /// <summary>Gets the character at the specified index.</summary>
    /// <param name="index">Index</param>
    /// <returns>Retruns the 7bit ascii character</returns>
    public byte this[int index] => Buffer[index];

    /// <summary>Copies the data to the specified buffer.</summary>
    /// <param name="buffer">Buffer to copy to</param>
    /// <param name="index">Index to start writing at.</param>
    public void CopyTo(byte[] buffer, int index) => Buffer.CopyTo(buffer, index);

    /// <inheritdoc/>
    public override string ToString() => ASCII.GetString(Buffer);

    /// <inheritdoc/>
    public override int GetHashCode() => DefaultHashingFunction.Calculate(Buffer);

    /// <inheritdoc/>
    public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)Buffer).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => Buffer.GetEnumerator();

    /// <summary>Cursor control</summary>
    public class Cursor
    {
        /// <summary>Moves the cursor n (default 1) cells in the given direction. If the cursor is already at the edge of the screen, this has no effect.</summary>
        /// <param name="n">1..</param>
        /// <returns>Returns the <see cref="Ansi"/> instance</returns>
        public static Ansi Up(int n = 1) => $"\x1B[{n}A";

        /// <summary>Moves the cursor n (default 1) cells in the given direction. If the cursor is already at the edge of the screen, this has no effect.</summary>
        /// <param name="n">1..</param>
        /// <returns>Returns the <see cref="Ansi"/> instance</returns>
        public static Ansi Down(int n = 1) => $"\x1B[{n}B";

        /// <summary>Moves the cursor n (default 1) cells in the given direction. If the cursor is already at the edge of the screen, this has no effect.</summary>
        /// <param name="n">1..</param>
        /// <returns>Returns the <see cref="Ansi"/> instance</returns>
        public static Ansi Forward(int n = 1) => $"\x1B[{n}C";

        /// <summary>Moves the cursor n (default 1) cells in the given direction. If the cursor is already at the edge of the screen, this has no effect.</summary>
        /// <param name="n">1..</param>
        /// <returns>Returns the <see cref="Ansi"/> instance</returns>
        public static Ansi Back(int n = 1) => $"\x1B[{n}D";

        /// <summary>Moves cursor to beginning of the line n (default 1) lines down</summary>
        /// <param name="n">1..</param>
        /// <returns>Returns the <see cref="Ansi"/> instance</returns>
        public static Ansi NextLine(int n = 1) => $"\x1B[{n}E";

        /// <summary>Moves cursor to beginning of the line n (default 1) lines up</summary>
        /// <param name="n">1..</param>
        /// <returns>Returns the <see cref="Ansi"/> instance</returns>
        public static Ansi PreviousLine(int n = 1) => $"\x1B[{n}F";

        /// <summary>Moves the cursor to column x (default 1). The values are 1-based, and default to 1 (left start)</summary>
        /// <param name="x">column x</param>
        /// <returns>Returns the <see cref="Ansi"/> instance</returns>
        public static Ansi X(int x = 1) => $"\x1B[{x}G";

        /// <summary>Moves the cursor to column x, row y. The values are 1-based, and default to 1 (top left corner) if omitted.</summary>
        /// <param name="x">column x</param>
        /// <param name="y">row y</param>
        /// <returns>Returns the <see cref="Ansi"/> instance</returns>
        public static Ansi XY(int x = 1, int y = 1) => $"\x1B[{y};{x}G";
    }

    /// <summary>Fe Escape sequences</summary>
    public class Control
    {
        /// <summary>
        /// Device Control String - Terminated by ST. Xterm's uses of this sequence include defining User-Defined Keys, and requesting or setting
        /// Termcap/Terminfo data.
        /// </summary>
        public static Ansi DCS => "\x1BP";

        /// <summary>Control Sequence Introducer - Starts most of the useful sequences, terminated by a byte in the range 0x40 through 0x7E.</summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Ansi CSI(byte code) => new([0x1B, (byte)'[', code]);

        /// <summary>String Terminator - Terminates strings in other controls.</summary>
        public static Ansi ST => "\x1B\\";

        /// <summary>Operating System Command - Starts a control string for the operating system to use, terminated by ST.</summary>
        public static Ansi OSC => "\x1B]";

        /// <summary>Sets the window title.</summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static Ansi Title(Ansi title) => OSC + $"0;{title}" + ST;
    }

    /// <summary>Clears part of the screen.</summary>
    /// <param name="screen">Allows to select the part of the screen to clear.</param>
    /// <returns>Returns the <see cref="Ansi"/> instance</returns>
    public static Ansi Clear(AnsiScreen screen = AnsiScreen.All) => $"\x1B[{(byte)screen}J";

    /// <summary>Clears part of the line.</summary>
    /// <param name="line">Allows to select the part of the line to clear.</param>
    /// <returns>Returns the <see cref="Ansi"/> instance</returns>
    public static Ansi Clear(AnsiLine line) => $"\x1B[{(byte)line}K";

    /// <summary>Scroll whole page up by n (default 1) lines. New lines are added at the bottom.</summary>
    /// <param name="n">number of lines</param>
    /// <returns>Returns the <see cref="Ansi"/> instance</returns>
    public static Ansi ScrollUp(int n = 1) => $"\x1B[{n}S";

    /// <summary>Scroll whole page down by n (default 1) lines. New lines are added at the top.</summary>
    /// <param name="n">number of lines</param>
    /// <returns>Returns the <see cref="Ansi"/> instance</returns>
    public static Ansi ScrollDown(int n = 1) => $"\x1B[{n}T";

    /// <summary>Sets the specified foreground and background color.</summary>
    /// <param name="fg">Foreground color</param>
    /// <param name="bg">Background color</param>
    /// <returns>Returns a new <see cref="Ansi"/> instance</returns>
    public static Ansi Color(AnsiColor8 fg, AnsiColor8 bg) => (FG)fg + (BG)bg;

    /// <summary>Sets the specified foreground and background color.</summary>
    /// <param name="fg">Foreground color</param>
    /// <param name="bg">Background color</param>
    /// <returns>Returns a new <see cref="Ansi"/> instance</returns>
    public static Ansi Color(AnsiColor16 fg, AnsiColor16 bg) => (FG)fg + (BG)bg;

    /// <summary>Sets the specified foreground and background color.</summary>
    /// <param name="fg">Foreground color</param>
    /// <param name="bg">Background color</param>
    /// <returns>Returns a new <see cref="Ansi"/> instance</returns>
    public static Ansi Color(AnsiColor256 fg, AnsiColor256 bg) => (FG)fg + (BG)bg;

    /// <summary>Sets the specified foreground and background color.</summary>
    /// <param name="fg">Foreground color</param>
    /// <param name="bg">Background color</param>
    /// <returns>Returns a new <see cref="Ansi"/> instance</returns>
    public static Ansi Color(AnsiColorRGB fg, AnsiColorRGB bg) => (FG)fg + (BG)bg;

    /// <summary>Resets Style and Color</summary>
    public static Ansi Reset => $"\x1B[m";

    /// <summary>Sets Font Style Bold.</summary>
    public static Ansi Bold => $"\x1B[1m";

    /// <summary>Sets Font Style Faint.</summary>
    public static Ansi Faint => $"\x1B[2m";

    /// <summary>Sets Font Style Italic.</summary>
    public static Ansi Italic => $"\x1B[3m";

    /// <summary>Sets Font Style Unterline.</summary>
    public static Ansi Underline => $"\x1B[4m";

    /// <summary>Sets Font Style Blink.</summary>
    public static Ansi Blink => $"\x1B[5m";

    /// <summary>Swap foreground and background colors</summary>
    public static Ansi Inverse => $"\x1B[7m";

    /// <summary>Sets Font Number <paramref name="n"/>.</summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public static Ansi Font(int n) => $"\x1B[{10 + n}m";

    /// <summary>Disables <see cref="Bold"/>, <see cref="Faint"/>, <see cref="Italic"/>, <see cref="Underline"/> and <see cref="Blink"/></summary>
    public static Ansi Normal => NotBoldOrFaint + NotItalic + NotUnderline + NotBlink;

    /// <summary>Disables <see cref="Bold"/> and <see cref="Faint"/> font styles.</summary>
    public static Ansi NotBoldOrFaint => $"\x1B[22m";

    /// <summary>Disables <see cref="Italic"/> font styles.</summary>
    public static Ansi NotItalic => $"\x1B[23m";

    /// <summary>Disables <see cref="Underline"/> font styles.</summary>
    public static Ansi NotUnderline => $"\x1B[25m";

    /// <summary>Disables <see cref="Blink"/> font styles.</summary>
    public static Ansi NotBlink => $"\x1B[25m";

    /// <summary>Set foreground color</summary>
    public class FG : Ansi
    {
        private FG(string content) : base(content) { }

        /// <summary>Converts the specified <paramref name="color"/> to an ansi escape code.</summary>
        /// <param name="color"></param>
        public static implicit operator FG(AnsiColor8 color) => new($"\x1B[{30 + (int)color}m");

        /// <summary>Converts the specified <paramref name="color"/> to an ansi escape code.</summary>
        public static implicit operator FG(AnsiColor16 color)
        {
            var code = (int)color;
            code = code < 8 ? code + 30 : code + 82;
            return new($"\x1B[{code}m");
        }

        /// <summary>Converts the specified <paramref name="color"/> to an ansi escape code.</summary>
        public static implicit operator FG(AnsiColor256 color) => new($"\x1B[38;5;{color.Index}m");

        /// <summary>Converts the specified <paramref name="color"/> to an ansi escape code.</summary>
        public static implicit operator FG(AnsiColorRGB color) => new($"\x1B[38;2;{color.Red};{color.Green};{color.Blue}m");

        /// <summary>Foreground color</summary>
        public static FG Black => AnsiColor16.Black;

        /// <summary>Foreground color</summary>
        public static FG DarkRed => AnsiColor16.DarkRed;

        /// <summary>Foreground color</summary>
        public static FG DarkGreen => AnsiColor16.DarkGreen;

        /// <summary>Foreground color</summary>
        public static FG DarkYellow => AnsiColor16.DarkYellow;

        /// <summary>Foreground color</summary>
        public static FG DarkBlue => AnsiColor16.DarkBlue;

        /// <summary>Foreground color</summary>
        public static FG DarkMagenta => AnsiColor16.DarkMagenta;

        /// <summary>Foreground color</summary>
        public static FG DarkCyan => AnsiColor16.DarkCyan;

        /// <summary>Foreground color</summary>
        public static FG Gray => AnsiColor16.Gray;

        /// <summary>Foreground color</summary>
        public static FG DarkGray => AnsiColor16.DarkGray;

        /// <summary>Foreground color</summary>
        public static FG Red => AnsiColor16.Red;

        /// <summary>Foreground color</summary>
        public static FG Green => AnsiColor16.Green;

        /// <summary>Foreground color</summary>
        public static FG Yellow = AnsiColor16.Yellow;

        /// <summary>Foreground color</summary>
        public static FG Blue => AnsiColor16.Blue;

        /// <summary>Foreground color</summary>
        public static FG Magenta => AnsiColor16.Magenta;

        /// <summary>Foreground color</summary>
        public static FG Cyan => AnsiColor16.Cyan;

        /// <summary>Foreground color</summary>
        public static FG White => AnsiColor16.White;

        /// <summary>Foreground color</summary>

        public static FG RGB(byte red, byte green, byte blue) => AnsiColorRGB.FromRgb(red, green, blue);

        /// <summary>Foreground color</summary>
        public static FG RGB(float red, float green, float blue) => AnsiColorRGB.FromRgb(red, green, blue);

        /// <summary>Foreground color</summary>
        public static FG Color(AnsiColor8 color) => color;

        /// <summary>Foreground color</summary>
        public static FG Color(AnsiColor16 color) => color;

        /// <summary>Foreground color</summary>
        public static FG Color(AnsiColor256 color) => color;

        /// <summary>Foreground color</summary>
        public static FG Color(AnsiColorRGB color) => color;
    }

    /// <summary>Set background color</summary>
    public class BG : Ansi
    {
        private BG(string content) : base(content) { }

        /// <summary>Converts the specified <paramref name="color"/> to an ansi escape code.</summary>
        public static implicit operator BG(AnsiColor8 color) => new($"\x1B[{40 + (int)color}m");

        /// <summary>Converts the specified <paramref name="color"/> to an ansi escape code.</summary>
        public static implicit operator BG(AnsiColor16 color)
        {
            var code = (int)color;
            code = code < 8 ? code + 40 : code + 92;
            return new($"\x1B[{code}m");
        }

        /// <summary>Converts the specified <paramref name="color"/> to an ansi escape code.</summary>
        public static implicit operator BG(AnsiColor256 color) => new($"\x1B[48;5;{color.Index}m");

        /// <summary>Converts the specified <paramref name="color"/> to an ansi escape code.</summary>
        public static implicit operator BG(AnsiColorRGB color) => new($"\x1B[48;2;{color.Red};{color.Green};{color.Blue}m");

        /// <summary>Foreground color</summary>
        public static BG Black => AnsiColor16.Black;

        /// <summary>Foreground color</summary>
        public static BG DarkRed => AnsiColor16.DarkRed;

        /// <summary>Foreground color</summary>
        public static BG DarkGreen => AnsiColor16.DarkGreen;

        /// <summary>Foreground color</summary>
        public static BG DarkYellow => AnsiColor16.DarkYellow;

        /// <summary>Foreground color</summary>
        public static BG DarkBlue => AnsiColor16.DarkBlue;

        /// <summary>Foreground color</summary>
        public static BG DarkMagenta => AnsiColor16.DarkMagenta;

        /// <summary>Foreground color</summary>
        public static BG DarkCyan => AnsiColor16.DarkCyan;

        /// <summary>Foreground color</summary>
        public static BG Gray => AnsiColor16.Gray;

        /// <summary>Foreground color</summary>
        public static BG DarkGray => AnsiColor16.DarkGray;

        /// <summary>Foreground color</summary>
        public static BG Red => AnsiColor16.Red;

        /// <summary>Foreground color</summary>
        public static BG Green => AnsiColor16.Green;

        /// <summary>Foreground color</summary>
        public static BG Yellow = AnsiColor16.Yellow;

        /// <summary>Foreground color</summary>
        public static BG Blue => AnsiColor16.Blue;

        /// <summary>Foreground color</summary>
        public static BG Magenta => AnsiColor16.Magenta;

        /// <summary>Foreground color</summary>
        public static BG Cyan => AnsiColor16.Cyan;

        /// <summary>Foreground color</summary>
        public static BG White => AnsiColor16.White;

        /// <summary>Foreground color</summary>
        public static BG RGB(byte red, byte green, byte blue) => AnsiColorRGB.FromRgb(red, green, blue);

        /// <summary>Foreground color</summary>
        public static BG RGB(float red, float green, float blue) => AnsiColorRGB.FromRgb(red, green, blue);

        /// <summary>Foreground color</summary>
        public static BG Color(AnsiColor8 color) => color;

        /// <summary>Foreground color</summary>
        public static BG Color(AnsiColor16 color) => color;

        /// <summary>Foreground color</summary>
        public static BG Color(AnsiColor256 color) => color;

        /// <summary>Foreground color</summary>
        public static BG Color(AnsiColorRGB color) => color;
    }
}
