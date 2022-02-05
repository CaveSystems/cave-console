using Cave.Logging;

namespace Cave.Console
{
    class LogSystemConsole : ILogTarget
    {
        #region Public Properties

        public bool Inverted { get => SystemConsole.Inverted; set => SystemConsole.Inverted = value; }

        public XTColor TextColor { get => SystemConsole.TextColor; set => SystemConsole.TextColor = value; }

        public XTStyle TextStyle { get => SystemConsole.TextStyle; set => SystemConsole.TextStyle = value; }

        public string Title { get => SystemConsole.Title; set => SystemConsole.Title = value; }

        #endregion Public Properties

        #region Public Methods

        public void Clear() => SystemConsole.Clear();

        public void NewLine() => SystemConsole.NewLine();

        public void ResetColor() => SystemConsole.ResetColor();

        public int Write(XT text) => SystemConsole.Write(text);

        public int Write(XTItem item) => SystemConsole.Write(item);

        public int WriteString(string text) => SystemConsole.WriteString(text);

        public void SetDefaultColors() => SystemConsole.SetDefaultColors();

        #endregion Public Methods
    }
}
