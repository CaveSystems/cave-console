namespace Cave.Console;

/// <summary>Ansi screen control</summary>
public enum AnsiScreen
{
    /// <summary>Clear from cursor to end of screen</summary>
    End = 0,

    /// <summary>Clear from cursor to begin of screen</summary>
    Start,

    /// <summary>Clear whole visible screen (does not clear scrollback)</summary>
    Visible,

    /// <summary>Clear whole visible screen and scrollback</summary>
    All,
}
