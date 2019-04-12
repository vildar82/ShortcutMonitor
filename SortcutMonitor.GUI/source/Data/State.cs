namespace ShortcutMonitor.GUI.Data
{
    using System;
    using System.Windows.Media;
    using NetLib.WPF;

    public class State : BaseModel
    {
        public static readonly Brush colorOk = new SolidColorBrush(Colors.Green);
        public static readonly Brush colorErr = new SolidColorBrush(Colors.Red);

        private State()
        {
        }

        public Project Project { get; set; }
        public ShortcutItem Item { get; set; }
        public string Status { get; set; }
        public Brush Color { get; set; }
        public string Msg { get; set; }
        public string Title { get; set; }
        public bool IsEmailErr { get; set; }
        public Action<State> Fix { get; set; }
        public string Author { get; set; }

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

        public static State Error(ShortcutItem item, Project project, string title, string msg, bool isEmailErr = false, Action<State> fix = null, string author = null)
        {
            return new State
            {
                Project = project,
                Item = item,
                Color = colorErr,
                Status = "Error",
                Msg = msg,
                Title = title,
                IsEmailErr = isEmailErr,
                Fix = fix,
                Author = author ?? item?.Author
            };
        }
    }
}
