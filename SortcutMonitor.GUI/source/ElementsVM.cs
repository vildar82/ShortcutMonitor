namespace ShortcutMonitor.GUI
{
    using System;
    using System.Reactive.Linq;
    using System.Text.RegularExpressions;
    using Data;
    using NetLib;
    using NetLib.IO;
    using NetLib.WPF;
    using ReactiveUI;

    public class ElementsVM : BaseModel
    {
        public ElementsVM(MainVM mainVm)
            : base(mainVm)
        {
            this.WhenAnyValue(v => v.Filter).Skip(1).Subscribe(s => Elements?.Reset());
            OpenProjectFolder = CreateCommand<ShortcutItem>(OpenProjectFolderExec);
            OpenSourceDwg = CreateCommand<ShortcutItem>(OpenSourceDwgExec);
        }

        public string Filter { get; set; }
        public IReactiveDerivedList<ShortcutItem> Elements { get; set; }
        public ReactiveCommand OpenProjectFolder { get; set; }
        public ReactiveCommand OpenSourceDwg { get; set; }
        public ReactiveCommand OpenElementFolder { get; set; }

        public void UpdateElements()
        {
            Elements = MainVM.AllElements.CreateDerivedCollection(s => s, OnFilter);
        }

        private bool OnFilter(ShortcutItem item)
        {
            if (Filter.IsNullOrEmpty() || item == null)
                return true;
            return Regex.IsMatch(item.ToString(), Filter, RegexOptions.IgnoreCase);
        }

        private void OpenProjectFolderExec(ShortcutItem item)
        {
            var dir = item.Project.Dir;
            dir.StartExplorer();
        }

        private void OpenSourceDwgExec(ShortcutItem item)
        {
            var file = item.SourceDwg;
            file.StartExplorer();
        }
    }
}
