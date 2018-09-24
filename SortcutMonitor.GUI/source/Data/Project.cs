using System.IO;
using JetBrains.Annotations;
using NetLib;
using NetLib.Monad;
using ShortcutMonitor.GUI.Xml;

namespace ShortcutMonitor.GUI.Data
{
    public class Project
    {
        public Project([NotNull] DirectoryInfo projectDir)
        {
            Dir = projectDir;
            Name = Dir.Name;
            var shortcutsDir = Dir.CreateSubdirectory("_Shortcuts");
            var historyXml = Path.Combine(shortcutsDir.FullName, "ShortcutsHistory.xml");
            var history = historyXml.Try(f => f?.FromXml<History>());
            Uuid = history?.ShortProjectID?.Uuid;
            Description = history?.ShortProjectID?.Desc;
        }

        /// <summary>
        /// Имя папки проекта быстрой ссылки
        /// </summary>
        public string Name { get; set; }

        public DirectoryInfo Dir { get; set; }

        /// <summary>
        /// Уникальный идентификатор проекта быстрой ссылки
        /// </summary>
        public string Uuid { get; set; }

        public string Description { get; set; }
    }
}