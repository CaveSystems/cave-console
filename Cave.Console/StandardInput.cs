using System.IO;
using Cave.IO;

namespace Cave.Console;

/// <summary>Access to standard input stream</summary>
public static class StandardInput
{
    #region Private Fields

    static Stream stream = System.Console.OpenStandardInput();

    #endregion Private Fields

    #region Public Constructors

    static StandardInput() => Codepage.Init();

    #endregion Public Constructors

    #region Public Properties

    /// <summary>Gets or sets the default <see cref="StringEncoding"/> for <see cref="GetReader(StringEncoding, NewLineMode, EndianType)"/> instances.</summary>
    /// <remarks>This is platform dependent by default!</remarks>
    public static StringEncoding DefaultEncoding { get; set; } = System.Console.InputEncoding.ToStringEncoding();

    /// <summary>Gets or sets the default <see cref="EndianType"/> for <see cref="GetReader(StringEncoding, NewLineMode, EndianType)"/> instances.</summary>
    /// <remarks>This is platform dependent by default!</remarks>
    public static EndianType DefaultEndianType { get; set; } = Endian.MachineType;

    /// <summary>Gets or sets the default <see cref="NewLineMode"/> for <see cref="GetReader(StringEncoding, NewLineMode, EndianType)"/> instances.</summary>
    /// <remarks>This is platform dependent by default!</remarks>
    public static NewLineMode DefaultNewLineMode { get; set; } = Platform.IsMicrosoft ? NewLineMode.CRLF : NewLineMode.LF;

    /// <summary>Checks whether the stream is available and can be read</summary>
    public static bool IsAvailable => Stream.CanRead;

    /// <summary>Gets the Stream instance</summary>
    public static Stream Stream
    {
        get
        {
            if (!stream.CanRead)
            {
                stream = System.Console.OpenStandardInput();
            }
            return stream;
        }
    }

    #endregion Public Properties

    #region Public Methods

    /// <summary>Gets a new <see cref="DataReader"/> instance for the <see cref="Stream"/>.</summary>
    /// <param name="encoding">Optional: Default <see cref="DefaultEncoding"/></param>
    /// <param name="newLineMode">Optional: Default <see cref="DefaultNewLineMode"/></param>
    /// <param name="endianType">Optional: Default <see cref="DefaultEndianType"/></param>
    /// <returns></returns>
    public static DataReader GetReader(StringEncoding encoding = 0, NewLineMode newLineMode = 0, EndianType endianType = 0)
        => new(Stream, encoding == 0 ? DefaultEncoding : encoding, newLineMode == 0 ? DefaultNewLineMode : newLineMode, endianType == 0 ? DefaultEndianType : endianType);

    #endregion Public Methods
}
