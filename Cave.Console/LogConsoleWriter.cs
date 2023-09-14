﻿using System.Collections.Generic;
using Cave.Logging;

namespace Cave.Console;

class LogConsoleWriter : LogWriter
{
    #region Public Methods

    public override void Write(LogMessage message, IEnumerable<ILogText> items) => SystemConsole.Write(items);

    #endregion Public Methods
}
