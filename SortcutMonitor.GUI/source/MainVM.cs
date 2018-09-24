namespace ShortcutMonitor.GUI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Data;
    using JetBrains.Annotations;
    using NetLib;
    using NetLib.IO;
    using NetLib.WPF;
    using ReactiveUI;

    public class MainVM : BaseViewModel
    {
        private bool inUpdate;
        public ReactiveList<ShortcutItem> _elements { get; set; }

        public MainVM()
        {
            this.WhenAnyValue(v => v.ShortcutFolder).Delay(TimeSpan.FromMilliseconds(200))
                .ObserveOn(dispatcher)
                .Subscribe(s => UpdateExecute());
            this.WhenAnyValue(v => v.Filter).Skip(1).Subscribe(s => Elements?.Reset());
            OpenProjectFolder = CreateCommand<ShortcutItem>(OpenProjectFolderExec);
            OpenSourceDwg = CreateCommand<ShortcutItem>(OpenSourceDwgExec);
            Update = CreateCommand(UpdateExecute);
        }

        public string ShortcutFolder { get; set; } = @"\\picompany.ru\root\ecp_wip\C3D_Projects";

        public string Filter { get; set; }

        public IReactiveDerivedList<ShortcutItem> Elements { get; set; }
        public ReactiveCommand OpenProjectFolder { get; set; }
        public ReactiveCommand OpenSourceDwg { get; set; }
        public ReactiveCommand Update { get; set; }

        private async void UpdateExecute()
        {
            try
            {
                if (inUpdate)
                    return;
                inUpdate = true;
                var files = new List<FileInfo>();
                await ShowProgressDialog(c =>
                {
                    c.SetIndeterminate();
                    if (ShortcutFolder == null || !Directory.Exists(ShortcutFolder))
                    {
                        _elements = null;
                        return;
                    }

                    files = GetShortcutFiles(ShortcutFolder).Result;
                }, "Загрузка элементов...", "", false);

                if (!files.Any())
                {
                    ShowMessage("Не найдены файлы быстрых ссылок (xml)");
                    return;
                }

                _elements = new ReactiveList<ShortcutItem>();
                Elements = _elements.CreateDerivedCollection(s => s, OnFilter);
                foreach (var xmls in files.ChunkBy(50))
                {
                    var items = await GetShortcutItem(xmls);
                    using (_elements.SuppressChangeNotifications())
                    {
                        items.ForEach(i => _elements.Add(i));
                    }
                }
            }
            finally
            {
                inUpdate = false;
            }
        }

        private Task<List<FileInfo>> GetShortcutFiles(string shortcutFolder)
        {
            return Task.Run(() =>
            {
                var di = new DirectoryInfo(ShortcutFolder);
                return di.EnumerateFiles("*.xml", SearchOption.AllDirectories)
                    .Where(IsShortcutXml)
                    .OrderByDescending(o => o.LastWriteTime).ToList();
            });
        }

        private bool IsShortcutXml(FileInfo xmlFile)
        {
            return xmlFile?.Directory?.Parent?.Name == "_Shortcuts";
        }

        [NotNull]
        private Task<List<ShortcutItem>> GetShortcutItem([NotNull] IEnumerable<FileInfo> xmlFiles)
        {
            return Task.Run(() =>
            {
                Thread.Sleep(50);
                return xmlFiles.Select(s => new ShortcutItem(s)).ToList();
            });
        }

        private bool OnFilter(ShortcutItem item)
        {
            if (Filter.IsNullOrEmpty() || item == null)
                return true;
            return Regex.IsMatch(item.ToString(), Filter, RegexOptions.IgnoreCase);
        }

        private void OpenProjectFolderExec(ShortcutItem item)
        {
            var dir = item.Project.Dir.FullName;
            dir.StartExplorer();
        }

        private void OpenSourceDwgExec(ShortcutItem item)
        {
            var file = item.SourceDwg;
            file.StartExplorer();
        }
    }
}