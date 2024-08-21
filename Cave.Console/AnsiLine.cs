namespace Cave.Console;

/// <summary>Line control</summary>
public enum AnsiLine
{
    /// <summary>Clear from cursor to end of line</summary>
    End = 0,

    /// <summary>Clear from cursor to begin of line</summary>
    Start,

    /// <summary>Clear the entire line</summary>
    All,
}
