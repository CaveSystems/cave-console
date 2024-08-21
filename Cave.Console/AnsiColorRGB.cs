#pragma warning disable CS0618 // obsolete functions

namespace Cave.Console;

/// <summary>Ansi 24 Bit RGB color</summary>
public struct AnsiColorRGB
{
    #region Public Fields

    /// <summary>Red color value [0..255]</summary>
    public byte Red;

    /// <summary>Green color value [0..255]</summary>
    public byte Green;

    /// <summary>Blue color value [0..255]</summary>
    public byte Blue;

    /// <summary>Red color value as float [0..1]</summary>
    public float RedFloat { get => Red / 255f; set => Red = (byte)(value * 255); }

    /// <summary>Green color value as float [0..1]</summary>
    public float GreenFloat { get => Green / 255f; set => Green = (byte)(value * 255); }

    /// <summary>Blue color value as float [0..1]</summary>
    public float BlueFloat { get => Blue / 255f; set => Blue = (byte)(value * 255); }

    #endregion Public Fields

    #region Public Methods

    /// <summary>Provides rgb conversion for ansi colors based on the 6 × 6 × 6 cube (216 colors)</summary>
    /// <param name="r">range 0..255</param>
    /// <param name="g">range 0..255</param>
    /// <param name="b">range 0..255</param>
    /// <returns>Returns a new <see cref="AnsiColorRGB"/> instance</returns>
    public static AnsiColorRGB FromRgb(byte r, byte g, byte b) => new() { Red = r, Green = g, Blue = b };

    /// <summary>Provides rgb conversion for ansi colors based on the 6 × 6 × 6 cube (216 colors)</summary>
    /// <param name="r">range 0..1</param>
    /// <param name="g">range 0..1</param>
    /// <param name="b">range 0..1</param>
    /// <returns>Returns a new <see cref="AnsiColorRGB"/> instance</returns>
    public static AnsiColorRGB FromRgb(float r, float g, float b) => new() { Red = (byte)(r * 255f), Green = (byte)(g * 255f), Blue = (byte)(b * 255f) };

    /// <summary>Gets a gray color based on the intensity.</summary>
    /// <param name="intensity">range 0..255</param>
    /// <returns>Returns a new <see cref="AnsiColorRGB"/> instance</returns>
    public static AnsiColorRGB GetGray(byte intensity) => new() { Red = intensity, Green = intensity, Blue = intensity };

    /// <summary>Gets a gray color based on the intensity.</summary>
    /// <param name="intensity">range 0..1</param>
    /// <returns>Returns a new <see cref="AnsiColorRGB"/> instance</returns>
    public static AnsiColorRGB GetGray(float intensity) => GetGray(intensity * 255f);

    #endregion Public Methods
}
