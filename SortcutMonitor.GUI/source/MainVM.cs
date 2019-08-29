using System.Windows;

namespace ShortcutMonitor.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using Data;
    using DynamicData;
    using JetBrains.Annotations;
    using Model;
    using NetLib;
    using NetLib.WPF;
    using ReactiveUI;
    using ToastNotifications;
    using ToastNotifications.Lifetime;
    using ToastNotifications.Position;

    public class MainVM : BaseViewModel
    {
        private bool inUpdate;
        public static readonly Brush eventBackground = new SolidColorBrush(Colors.Yellow);
        public static readonly Brush eventNewBackground = new SolidColorBrush(Colors.LimeGreen);
        public static readonly Brush eventDeleteBackground = new SolidColorBrush(Colors.SandyBrown);
        private FileWatcherRx projectWatcher;
        public static SourceList<ShortcutItem> AllElements { get; set; } = new SourceList<ShortcutItem>();
        public static SourceList<Project> AllProjects { get; set; } = new SourceList<Project>();

        public MainVM()
        {
            MainVm = this;
            Notify = new Notifier(cfg =>
            {
                cfg.PositionProvider = new PrimaryScreenPositionProvider(
                    corner: Corner.TopRight,
                    offsetX: 10,
                    offsetY: 30);
                cfg.LifetimeSupervisor = new CountBasedLifetimeSupervisor(
                    maximumNotificationCount: MaximumNotificationCount.UnlimitedNotifications());
                cfg.DisplayOptions.Width = 800;
                cfg.Dispatcher = dispatcher;
            });
            ElementsVM = new ElementsVM(this);
            ProjectsVM = new ProjectsVM(this);
#if DEBUG
            ShortcutFolder = @"d:\vildar\test\GP\C3D_Projects";
#endif
            this.WhenAnyValue(v => v.ShortcutFolder).Delay(TimeSpan.FromMilliseconds(200))
                .ObserveOn(dispatcher)
                .Subscribe(s => UpdateExecute());
            Update = CreateCommand(UpdateExecute);
        }

        public static MainVM MainVm { get; set; }
        public string ShortcutFolder { get; set; } = @"\\picompany.ru\root\ecp_wip\C3D_Projects";
        public ElementsVM ElementsVM { get; set; }
        public ProjectsVM ProjectsVM { get; set; }
        public ReactiveCommand<Unit, Unit> Update { get; set; }
        public static Notifier Notify { get; set; }

        private async void UpdateExecute()
        {
            try
            {
                projectWatcher?.Watcher?.Dispose();
                if (inUpdate || !Directory.Exists(ShortcutFolder)) return;

//                projectWatcher = new FileWatcherRx(ShortcutFolder, "", (NotifyFilters) 19, WatcherChangeTypes.All);
//                projectWatcher.Watcher.IncludeSubdirectories = true;
//                projectWatcher.Created.ObserveOn(dispatcher).Subscribe(s => OnCreatedProject(s.EventArgs));
//                projectWatcher.Deleted.ObserveOn(dispatcher).Subscribe(s => OnDeletedProject(s.EventArgs));
//                projectWatcher.Renamed.ObserveOn(dispatcher).Subscribe(s => OnRenamedProject(s.EventArgs));
//                projectWatcher.Changed.ObserveOn(dispatcher).Subscribe(s => OnChangedProject(s.EventArgs));
                inUpdate = true;
                var files = new List<FileInfo>();
                AllElements.Clear();
                AllProjects.Clear();
                await ShowProgressDialog(c =>
                {
                    c.SetIndeterminate();
                    if (ShortcutFolder == null || !Directory.Exists(ShortcutFolder))
                    {
                        return;
                    }

                    files = GetShortcutFiles(ShortcutFolder).Result;
                }, "Загрузка элементов...", "", false);

                if (!files.Any())
                {
                    ShowMessage("Не найдены файлы быстрых ссылок (xml)");
                    return;
                }

                foreach (var xmls in files.ChunkBy(50))
                {
                    var items = await GetShortcutItems(xmls);
                    foreach (var i in items)
                    {
                        Checks.CheckElement(i);
                        i.Project.Shortcuts.Add(i);
                        AllElements.Add(i);
                    }
                }

                var projects = Project.GetProjects();
                AllProjects.AddRange(projects);
                ProjectsVM.UpdateProjects();
            }
            catch (Exception ex)
            {
                ShowMessage(ex.ToString());
            }
            finally
            {
                inUpdate = false;
            }
        }

        public static Task<List<FileInfo>> GetShortcutFiles(string shortcutFolder)
        {
            return Task.Run(() =>
            {
                try
                {
                    var di = new DirectoryInfo(shortcutFolder);
                    return di.EnumerateFiles("*.xml", SearchOption.AllDirectories)
                        .Where(IsShortcutXml)
                        .OrderByDescending(o => o.LastWriteTime).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return new List<FileInfo>();
                }
            });
        }

        private static bool IsShortcutXml(FileInfo xmlFile)
        {
            return xmlFile?.Directory?.Parent?.Name == "_Shortcuts";
        }

        [NotNull]
        public static Task<List<ShortcutItem>> GetShortcutItems([NotNull] IEnumerable<FileInfo> xmlFiles)
        {
            return Task.Run(() =>
            {
                return xmlFiles.Select(s =>
                {
                    var item = new ShortcutItem(s);
                    return item;
                }).ToList();
            });
        }

        private async void OnCreatedProject(FileSystemEventArgs e)
        {
            Debug.WriteLine($"OnCreatedProject - {e.Name}, {e.FullPath}.");
            //// Событие создания проекта
            //Notify.ShowSuccess($"Создан проект: {e.Name}, {DateTime.Now}.");
            //var project = Project.GetProject(new DirectoryInfo(e.FullPath), true);
            //var xmlFiles = await GetShortcutFiles(e.FullPath);
            //var items = await GetShortcutItems(xmlFiles);
            //if (items.Count == 0)
            //{
            //    project.SubscribeShortcutsFolder();
            //}
            //else
            //{
            //    foreach (var i in items)
            //    {
            //        i.Background = eventNewBackground;
            //        i.Events.Add($"Создан {DateTime.Now}");
            //        AllElements.Insert(0, i);
            //    }
            //}
        }

        private void OnDeletedProject(FileSystemEventArgs e)
        {
            Debug.WriteLine($"OnDeletedProject - {e.Name}, {e.FullPath}.");

            //// Событие удаления проекта
            // var msg = $"Удален проект: {e.Name}, {DateTime.Now}.";
            // Notify.ShowError(msg);
            // if (Project.Projects.TryGetValue(e.Name, out var project))
            // {
            //    project.Events.Add(msg);
            //    project.Background = eventDeleteBackground;
            //    project.Shortcuts = new List<ShortcutItem>();
            // }
        }

        private void OnRenamedProject(RenamedEventArgs e)
        {
            Debug.WriteLine($"OnRenamedProject - {e.OldName}->{e.Name}, {e.FullPath}.");

            //// Событие переименования проекта
            // var msg = $"Переименован проект: {e.OldName} -> {e.Name}, {DateTime.Now}.";
            // Notify.ShowWarning(msg);
            // if (Project.Projects.TryGetValue(e.OldName, out var project))
            // {
            //    project.Name = e.Name;
            //    project.Dir = e.FullPath;
            //    project.Events.Add(msg);
            //    project.Background = eventBackground;
            // }
        }

        private void OnChangedProject(FileSystemEventArgs e)
        {
            Debug.WriteLine($"OnChangedProject - {e.Name}, {e.FullPath}.");

            // var msg = $"OnChangedProject: {e.Name}, {e.FullPath}, {DateTime.Now}.";
            // Notify.ShowWarning(msg);
        }
    }
}
