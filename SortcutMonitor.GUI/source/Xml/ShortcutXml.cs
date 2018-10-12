using System.Xml.Serialization;

namespace ShortcutMonitor.GUI.Xml
{
    /// <summary>
    /// Файл элемента быстрой ссылки
    /// </summary>
    public class ProjectInfo
    {
        public string ProjectDesc { get; set; }
        public Shortcuts Shortcuts { get; set; }
        public string Dependencies { get; set; }
    }

    public class Shortcuts
    {
        public Shortcut Shortcut { get; set; }
    }

    public class Shortcut
    {
        public DwgRelPath DwgRelPath { get; set; }
        public Criteria Criteria { get; set; }
        [XmlAttribute(AttributeName="name")]
        public string Name { get; set; }
    }

    public class DwgRelPath
    {
        [XmlAttribute(AttributeName="path")]
        public string Path { get; set; }
    }

    public class Criteria
    {
        public File File { get; set; }
        public Object Object { get; set; }
        public DisplayProperties DisplayProperties { get; set; }
    }

    public class File
    {
        [XmlAttribute(AttributeName="name")]
        public string Name { get; set; }
    }

    public class Object
    {
        [XmlAttribute(AttributeName="objectDescription")]
        public string ObjectDescription { get; set; }
        [XmlAttribute(AttributeName="type")]
        public string Type { get; set; }
        public string UseType { get; set; }
        [XmlAttribute(AttributeName="name")]
        public string Name { get; set; }
        public string UseName { get; set; }
        public string Version { get; set; }
        public string UseVersion { get; set; }
        public string HandleLow { get; set; }
        public string HandleHigh { get; set; }
        public string UseHandle { get; set; }
        public string ParentHandleHigh { get; set; }
        public string ParentHandleLow { get; set; }
        public string ParentAlignmentShortcutName { get; set; }
        public string LastValidateResult { get; set; }
        public string Visable { get; set; }
    }

    public class DisplayProperties
    {
        [XmlAttribute(AttributeName="layer")]
        public string Layer { get; set; }
        public string UseLayer { get; set; }
        public string Color { get; set; }
        public string UseColor { get; set; }
        public string LineType { get; set; }
        public string UseLineType { get; set; }
        public string LineWeight { get; set; }
        public string UseLineWeight { get; set; }
    }
}