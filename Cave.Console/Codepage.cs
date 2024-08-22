using System;
using System.Collections.Generic;
using System.Text;

namespace Cave.Console;

static class Codepage
{
#if NET5_0_OR_GREATER
    static bool done;

    public static void Init()
    {
        if (!done)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            done = true;
        }
    }
#else

    public static void Init() { }

#endif
}
