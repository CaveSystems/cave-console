using System;

#pragma warning disable CS0618 // obsolete functions

namespace Cave.Console;

/// <summary>Ansi 256 colors</summary>
public struct AnsiColor256
{
    #region Public Fields

    /// <summary>Ansi 256 color table index.</summary>
    public byte Index;

    #endregion Public Fields

    /// <summary>Converts from index to <see cref="AnsiColor256"/>.</summary>
    /// <param name="index"></param>
    public static implicit operator AnsiColor256(byte index) => new() { Index = index };

    #region Public Methods

    /// <summary>Provides rgb conversion for ansi colors based on the 6 × 6 × 6 cube (216 colors)</summary>
    /// <param name="r">range 0..255</param>
    /// <param name="g">range 0..255</param>
    /// <param name="b">range 0..255</param>
    /// <returns>Returns a new <see cref="AnsiColor256"/> instance</returns>
    public static AnsiColor256 FromRgb(byte r, byte g, byte b) => new() { Index = (byte)(16 + (36 * r / 51) + (6 * g / 51) + (b / 51)) };

    /// <summary>Provides rgb conversion for ansi colors based on the 6 × 6 × 6 cube (216 colors)</summary>
    /// <param name="r">range 0..1</param>
    /// <param name="g">range 0..1</param>
    /// <param name="b">range 0..1</param>
    /// <returns>Returns a new <see cref="AnsiColor256"/> instance</returns>
    public static AnsiColor256 FromRgb(float r, float g, float b) => new() { Index = (byte)((16 + (244 * r) + (24 * g) + (4 * b))) };

    /// <summary>Converts the specified <paramref name="color"/> to an <see cref="AnsiColor256"/>.</summary>
    /// <param name="color">Color to convert.</param>
    /// <returns>Returns a new <see cref="AnsiColor256"/> instance</returns>
    public static AnsiColor256 FromTerminalColor(AnsiColor16 color) => new() { Index = (byte)color };

    /// <summary>Gets a gray color based on the intensity.</summary>
    /// <param name="intensity">range 0..255</param>
    /// <returns>Returns a new <see cref="AnsiColor256"/> instance</returns>
    public static AnsiColor256 GetGray(byte intensity) => new() { Index = (byte)(232 + ((intensity * 24) / 255)) };

    /// <summary>Gets a gray color based on the intensity.</summary>
    /// <param name="intensity">range 0..1</param>
    /// <returns>Returns a new <see cref="AnsiColor256"/> instance</returns>
    public static AnsiColor256 GetGray(float intensity) => GetGray(intensity * 255f);

    #endregion Public Methods

    /// <summary>Red color value [0..5]</summary>
    public byte Red => (byte)(((Index - 16) / 36) * 51);

    /// <summary>Green color value [0..5]</summary>
    public byte Green => (byte)((((Index - 16) % 36) / 6) * 51);

    /// <summary>Blue color value [0..5]</summary>
    public byte Blue => (byte)(((Index - 16) % 6) * 51);

    /// <summary>Red color value as float [0..1]</summary>
    public float RedFloat => Red / 255f;

    /// <summary>Green color value as float [0..1]</summary>
    public float GreenFloat => Green / 255f;

    /// <summary>Blue color value as float [0..1]</summary>
    public float BlueFloat => Blue / 255f;

    /// <summary>Converts to an <see cref="AnsiColorRGB"/> value.</summary>
    /// <returns>Returns the matching <see cref="AnsiColorRGB"/>.</returns>
    public AnsiColorRGB ToAnsiColorRGB() => new AnsiColorRGB() { Red = Red, Green = Green, Blue = Blue };
}
