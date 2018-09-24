using System.Collections.Generic;

namespace ShortcutMonitor.GUI.Xml
{
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