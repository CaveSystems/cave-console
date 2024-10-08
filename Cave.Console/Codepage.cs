using System;
using System.Collections.Generic;
using System.Text;

namespace Cave.Console;

static class Codepage
{
#if NET5_0_OR_GREATER

    #region Private Fields

    static bool done;

    #endregion Private Fields

    #region Public Methods

    public static void Init()
    {
        if (!done)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            done = true;
        }
    }

    #endregion Public Methods

#else

    public static void Init() { }

#endif
}
