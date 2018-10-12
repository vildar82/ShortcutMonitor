namespace ShortcutMonitor.GUI.Data
{
    using System;
    using System.Windows.Media;
    using ReactiveUI;

    public interface IEventItem
    {
        ReactiveList<string> Events { get; set; }

        Brush Background { get; set; }

        string Author { get; set; }

        DateTime LastWriteDate { get; set; }
    }
}
