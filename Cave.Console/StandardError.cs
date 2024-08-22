using System.IO;
using Cave.IO;

namespace Cave.Console;

/// <summary>Access to standard error stream</summary>
public static class StandardError
{
    #region Private Fields

    static Stream stream = System.Console.OpenStandardOutput();

    #endregion Private Fields

    #region Public Constructors

    static StandardError() => Codepage.Init();

    #endregion Public Constructors

    #region Public Properties

    /// <summary>Gets or sets the default <see cref="StringEncoding"/> for <see cref="GetWriter(StringEncoding, NewLineMode, EndianType)"/> instances.</summary>
    /// <remarks>This is platform dependent by default!</remarks>
    public static StringEncoding DefaultEncoding { get; set; } = System.Console.OutputEncoding.ToStringEncoding();

    /// <summary>Gets or sets the default <see cref="EndianType"/> for <see cref="GetWriter(StringEncoding, NewLineMode, EndianType)"/> instances.</summary>
    /// <remarks>This is platform dependent by default!</remarks>
    public static EndianType DefaultEndianType { get; set; } = Endian.MachineType;

    /// <summary>Gets or sets the default <see cref="NewLineMode"/> for <see cref="GetWriter(StringEncoding, NewLineMode, EndianType)"/> instances.</summary>
    /// <remarks>This is platform dependent by default!</remarks>
    public static NewLineMode DefaultNewLineMode { get; set; } = Platform.IsMicrosoft ? NewLineMode.CRLF : NewLineMode.LF;

    /// <summary>Checks whether the stream is available and can be read</summary>
    public static bool IsAvailable => Stream.CanWrite;

    /// <summary>Gets the Stream instance</summary>
    public static Stream Stream
    {
        get
        {
            if (!stream.CanWrite)
            {
                stream = System.Console.OpenStandardError();
            }
            return stream;
        }
    }

    #endregion Public Properties

    #region Public Methods

    /// <summary>Gets a new <see cref="DataWriter"/> instance for the <see cref="Stream"/>.</summary>
    /// <param name="encoding">Optional: Default <see cref="DefaultEncoding"/></param>
    /// <param name="newLineMode">Optional: Default <see cref="DefaultNewLineMode"/></param>
    /// <param name="endianType">Optional: Default <see cref="DefaultEndianType"/></param>
    /// <returns></returns>
    public static DataWriter GetWriter(StringEncoding encoding = 0, NewLineMode newLineMode = 0, EndianType endianType = 0)
        => new(Stream, encoding == 0 ? DefaultEncoding : encoding, newLineMode == 0 ? DefaultNewLineMode : newLineMode, endianType == 0 ? DefaultEndianType : endianType);

    #endregion Public Methods
}
