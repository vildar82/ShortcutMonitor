namespace SortcutMonitor.GUI.Xml
{
    using System.Collections.Generic;

    /// <summary>
    /// Файл ShortcutsFolders.xml
    /// </summary>
    public class ShortcutFolderXml
    {
        public List<RootFolder> ShortcutFolders { get; set; }
    }

    public class RootFolder
    {
        public string EntityType { get; set; }
        public string AlignType { get; set; }
    }
}