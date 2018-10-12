namespace ShortcutMonitor.GUI.Data
{
    using System.Windows.Media;

    public class State
    {
        private static readonly Brush colorOk = new SolidColorBrush(Colors.Green);
        private static readonly Brush colorErr = new SolidColorBrush(Colors.Red);

        private State()
        {
        }

        public ShortcutItem Item { get; set; }
        public string Status { get; set; }
        public Brush Color { get; set; }
        public string Msg { get; set; }
        public string Title { get; set; }

        public static State Ok(ShortcutItem item, string title, string msg)
        {
            return new State
            {
                Item = item,
                Color = colorOk,
                Status = "OK",
                Title = title,
                Msg = msg
            };
        }

        public static State Error(ShortcutItem item, string title, string msg)
        {
            return new State
            {
                Item = item,
                Color = colorErr,
                Status = "Error",
                Msg = msg,
                Title = title
            };
        }
    }
}
