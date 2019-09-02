using System.Collections.Generic;
using System.Linq;
using NetLib.IO;

namespace ShortcutMonitor.GUI.Data
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows.Media;
    using JetBrains.Annotations;
    using NetLib;
    using NetLib.Monad;
    using NetLib.WPF;
    using ToastNotifications.Messages;
    using Xml;

    /// <summary>
    /// Элемент быстрой ссылки
    /// </summary>
    public class ShortcutItem : BaseModel, IEventItem
    {
        public ShortcutItem([NotNull] FileInfo xmlFile)
        {
            XmlFile = xmlFile.FullName;
            XmlFileName = xmlFile.Name;
            LastWriteDate = xmlFile.LastWriteTime;
            Project = Project.GetProject(xmlFile.Directory.Parent.Parent, true);
            Group = xmlFile.Directory.Name;
            ProjectXml = xmlFile.FullName.Try(f => f?.FromXml<ProjectInfo>());
            var shc = ProjectXml?.Shortcuts?.Shortcut;
            if (shc != null)
            {
                Name = shc.Name;
                DwgRelPath = shc.DwgRelPath?.Path;
                SourceDwg = shc.Criteria?.File?.Name;
                var obj = shc.Criteria?.Object;
                ElementName = obj?.Name;
                ElementType = obj?.Type;
                ElementDescription = obj?.ObjectDescription;
                ElementLayer = shc.Criteria?.DisplayProperties?.Layer;
                Author = SourceDwg.Author();
            }
            else
            {
                Author = xmlFile.FullName.Author();
            }
        }

        public ProjectInfo ProjectXml { get; set; }
        public Project Project { get; set; }
        public string Group { get; set; }

        /// <summary>
        /// Файл описания быстрой ссылки
        /// </summary>
        public string XmlFile { get; set; }

        /// <summary>
        /// Файл описания быстрой ссылки
        /// </summary>
        public string XmlFileName { get; set; }
        public string Author { get; set; }
        public DateTime LastWriteDate { get; set; }

        /// <summary>
        /// Shortcut name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// DwgRelPath
        /// </summary>
        public string DwgRelPath { get; set; }

        /// <summary>
        /// Источник объекта внешней ссылки - файл dwg
        /// </summary>
        public string SourceDwg { get; set; }

        /// <summary>
        /// Название элемента быстрой ссылки - имя поверхности и т.п.
        /// </summary>
        public string ElementName { get; set; }

        /// <summary>
        /// Тип элемента быстрой ссылки - AeccDbSurfaceTin
        /// </summary>
        public string ElementType { get; set; }

        /// <summary>
        /// Примечание к элементу
        /// </summary>
        public string ElementDescription { get; set; }

        /// <summary>
        /// Слой элемента
        /// </summary>
        public string ElementLayer { get; set; }
        public ObservableCollection<string> Events { get; set; } = new ObservableCollection<string>();
        public Brush Background { get; set; }
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Проверки
        /// </summary>
        public List<State> Status { get; set; } = new List<State>();

        public bool HasError => Status?.Any() == true;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Project?.Name} {ElementName} {Group} {Author} {SourceDwg}";
        }

        private void ShortcutChanged(FileSystemEventArgs e)
        {
            var item = new ShortcutItem(new FileInfo(e.FullPath));
            if (item.XmlFileName != XmlFileName)
            {
                var msg = $"Переименован файл '{XmlFileName}' -> '{item.XmlFileName}'.";
                Events.Add(msg);
                Background = MainVM.eventBackground;
                MainVM.Notify.ShowWarning($"{msg} {Project.Name}.");
                XmlFile = item.XmlFile;
                XmlFileName = item.XmlFileName;
            }

            if (item.Name != Name)
            {
                var msg = $"Переименован ключ '{Name}' -> '{item.Name}'.";
                Events.Add(msg);
                Background = MainVM.eventBackground;
                MainVM.Notify.ShowWarning($"{msg} {Project.Name}.");
                Name = item.Name;
            }

            if (item.ElementName != ElementName)
            {
                var msg = $"Переименован объект '{ElementName}' -> '{item.ElementName}'.";
                Events.Add(msg);
                Background = MainVM.eventBackground;
                MainVM.Notify.ShowWarning($"{msg} {Project.Name}.");
            }

            if (item.SourceDwg != SourceDwg)
            {
                var msg = $"Переименован источник '{SourceDwg}' -> '{item.SourceDwg}'.";
                Events.Add(msg);
                Background = MainVM.eventBackground;
                MainVM.Notify.ShowWarning($"{msg} {Project.Name}.");
            }
        }
    }
}
