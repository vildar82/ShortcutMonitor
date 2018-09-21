namespace SortcutMonitor.GUI.Data
{
    using System.IO;
    using JetBrains.Annotations;
    using NetLib;
    using NetLib.Monad;
    using Xml;

    /// <summary>
    /// Элемент быстрой ссылки
    /// </summary>
    public class ShortcutItem
    {
        public ShortcutItem([NotNull] FileInfo xmlFile)
        {
            XmlFile = xmlFile;
            Project = new Project(xmlFile.Directory.Parent.Parent);
            Group = xmlFile.Directory.Name;
            Author = xmlFile.FullName.Try(f =>
                System.IO.File.GetAccessControl(f).GetOwner(typeof(System.Security.Principal.NTAccount)).ToString());
            var shortcutXml = xmlFile.FullName.Try(f => f?.FromXml<ProjectInfo>());
            var shc = shortcutXml?.Shortcuts?.Shortcut;
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
            }
        }

        public Project Project { get; set; }
        public string Group { get; set; }

        /// <summary>
        /// Файл описания быстрой ссылки
        /// </summary>
        public FileInfo XmlFile { get; set; }

        public string Author { get; set; }

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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Project?.Name} {ElementName} {Group} {Author} {SourceDwg}";
        }
    }
}