using System;
using Cave.Logging;

namespace Cave.Console;

/// <summary>Interface for a log console.</summary>
public interface ILogConsole : ILogReceiver
{
    /// <summary>Gets or sets the title of the logconsole.</summary>
    string Title { get; set; }

    /// <summary>Clears the terminal.</summary>
    void Clear();
}
