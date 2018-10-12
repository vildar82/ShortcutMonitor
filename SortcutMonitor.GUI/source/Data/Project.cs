namespace ShortcutMonitor.GUI.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using JetBrains.Annotations;
    using NetLib;
    using NetLib.IO;
    using NetLib.Monad;
    using NetLib.WPF;
    using ReactiveUI;
    using ToastNotifications.Messages;
    using Xml;

    public class Project : BaseModel, IEventItem
    {
        private string shortcutsDir;
        private static Dictionary<string, Project> dictProjects { get; set; } = new Dictionary<string, Project>();

        private Project([NotNull] DirectoryInfo projectDir)
        {
            Dir = projectDir.FullName;
            Author = Dir.Author();
            LastWriteDate = projectDir.LastWriteTime;
            Name = projectDir.Name;
            SubscribeShortcutsFolder();
        }

        public async void SubscribeShortcutsFolder()
        {
            shortcutsDir = System.IO.Path.Combine(Dir, "_Shortcuts");
            var existShortcutsDir = Directory.Exists(shortcutsDir);
            if (!existShortcutsDir)
            {
                await Task.Run(() =>
                {
                    while (!Directory.Exists(shortcutsDir))
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(15));
                    }
                    return;
                });
            }

            var historyXml = System.IO.Path.Combine(shortcutsDir, "ShortcutsHistory.xml");
            var history = historyXml.Try(f => f?.FromXml<History>());
            Uuid = history?.ShortProjectID?.Uuid;
            Description = history?.ShortProjectID?.Desc;
            if (!existShortcutsDir)
                Update();
        }

        /// <summary>
        /// Имя папки проекта быстрой ссылки
        /// </summary>
        public string Name { get; set; }

        public string Dir { get; set; }

        /// <summary>
        /// Уникальный идентификатор проекта быстрой ссылки
        /// </summary>
        public string Uuid { get; set; }

        public string Description { get; set; }

        public ReactiveList<string> Events { get; set; } = new ReactiveList<string>();

        public Brush Background { get; set; }

        /// <inheritdoc />
        public string Author { get; set; }

        /// <inheritdoc />
        public DateTime LastWriteDate { get; set; }

        [NotNull]
        public List<ShortcutItem> Shortcuts { get; set; } = new List<ShortcutItem>();

        /// <summary>
        /// Проверки
        /// </summary>
        public List<State> Status { get; set; } = new List<State>();

        public static Project GetProject(DirectoryInfo dir, bool createNew)
        {
            if (dictProjects.TryGetValue(dir.Name, out var project) || !createNew)
                return project;
            project = new Project(dir);
            dictProjects.Add(dir.Name, project);
            return project;
        }

        public static List<Project> GetProjects()
        {
            return dictProjects.Values.ToList();
        }

        private async void Update()
        {
            // Обновление элементов быстрых ссылок
            var filesXml = await MainVM.GetShortcutFiles(shortcutsDir);
            var items = await MainVM.GetShortcutItems(filesXml);
            foreach (var shortcut in Shortcuts.ToList())
            {
                if (!System.IO.File.Exists(shortcut.XmlFile) && !shortcut.IsDeleted)
                {
                    shortcut.IsDeleted = true;
                    shortcut.Background = MainVM.eventDeleteBackground;
                    shortcut.Events.Add($"Удалено {DateTime.Now}.");
                    Shortcuts.Remove(shortcut);
                    MainVM.Notify.ShowError($"Удалена быстрая ссылка {shortcut.Name}, {Name}.");
                }
            }

            foreach (var item in items)
            {
                var shortcut = Shortcuts.FirstOrDefault(i => i.XmlFile == item.XmlFile);
                if (shortcut == null)
                {
                    shortcut = Shortcuts.FirstOrDefault(i => i.Name == item.Name);
                    if (shortcut != null)
                    {
                        // Переименовался файл
                        shortcut.Background = MainVM.eventBackground;
                        var msg = $"Переименован файл xml {shortcut.XmlFileName} -> {item.XmlFileName}.";
                        shortcut.Events.Add($"{msg} {Name}.");
                        MainVM.Notify.ShowWarning($"{msg} {Name}.");
                        continue;
                    }

                    item.Background = MainVM.eventNewBackground;
                    item.Events.Add($"Создан {DateTime.Now}.");
                    MainVM.AllElements.Insert(0, item);
                    MainVM.Notify.ShowSuccess($"Создан элемент {item.ElementName}. {Name}");
                    item.Project.Shortcuts.Add(item);
                    Shortcuts.Insert(0, item);
                }
                else
                {
                    if (item.Name != shortcut.Name)
                    {
                        // Изменилось ключевое имя быстрой ссылки
                        var msg = $"Переименован ключ быстрой ссылки {shortcut.Name} -> {item.Name}.";
                        shortcut.Events.Add(msg);
                        MainVM.Notify.ShowWarning($"{msg} {Name}.");
                        shortcut.Name = item.Name;
                    }

                    if (item.ElementName != shortcut.ElementName)
                    {
                        // Изменилось имя объекта быстрой ссылки
                        var msg = $"Переименован объект быстрой ссылки {shortcut.ElementName} -> {item.ElementName}.";
                        shortcut.Events.Add(msg);
                        MainVM.Notify.ShowWarning($"{msg} {Name}.");
                        shortcut.ElementName = item.ElementName;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} {Dir} {Description}";
        }
    }
}