using System.Text;

namespace Cave.Console;

static class LogConsoleFlagsExtension
{
    #region Public Methods

    public static string ToMessageFormat(this LogConsoleFlags flags)
    {
        var format = new StringBuilder();
        format.Append("<inverse>{LevelColor}");
        var prefix = "";
        if (flags.HasFlag(LogConsoleFlags.DisplayOneLetterLevel))
        {
            format.Append($"{prefix}{{ShortLevel}}");
            prefix = " ";
        }

        var date = flags.HasFlag(LogConsoleFlags.DisplayDate);
        var time = flags.HasFlag(LogConsoleFlags.DisplayTimeStamp);
        if (date)
        {
            if (time)
            {
                format.Append($"{prefix}{{DateTime}}");
                prefix = " ";
            }
            else
            {
                format.Append($"{prefix}{{Date}}");
                prefix = " ";
            }
        }
        else if (time)
        {
            format.Append($"{prefix}{{Time}}");
            prefix = " ";
        }

        if (flags.HasFlag(LogConsoleFlags.DisplayLongLevel))
        {
            format.Append($"{prefix}{{Level}}");
            prefix = " ";
        }
        format.Append($"{prefix}{{Sender}}><reset> {{Content}}");
        if (flags.HasFlag(LogConsoleFlags.DisplaySource))
        {
            format.Append(" <inverse><blue>@{SourceFile}({SourceLine}): {SourceMember}");
        }
        format.Append('\n');
        return format.ToString();
    }

    #endregion Public Methods
}
