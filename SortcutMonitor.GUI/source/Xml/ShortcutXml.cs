namespace ShortcutMonitor.GUI.Xml
{
    using System.Xml.Serialization;

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
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    public class DwgRelPath
    {
        [XmlAttribute(AttributeName = "path")]
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
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    public class Object
    {
        [XmlAttribute(AttributeName = "objectDescription")]
        public string ObjectDescription { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "useType")]
        public string UseType { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "useName")]
        public string UseName { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "useVersion")]
        public string UseVersion { get; set; }
        [XmlAttribute(AttributeName = "handleLow")]
        public string HandleLow { get; set; }
        [XmlAttribute(AttributeName = "handleHigh")]
        public string HandleHigh { get; set; }
        [XmlAttribute(AttributeName = "useHandle")]
        public string UseHandle { get; set; }
        [XmlAttribute(AttributeName = "parentHandleHigh")]
        public string ParentHandleHigh { get; set; }
        [XmlAttribute(AttributeName = "parentHandleLow")]
        public string ParentHandleLow { get; set; }
        [XmlAttribute(AttributeName = "parentAlignmentShortcutName")]
        public string ParentAlignmentShortcutName { get; set; }
        [XmlAttribute(AttributeName = "lastValidateResult")]
        public string LastValidateResult { get; set; }
        [XmlAttribute(AttributeName = "visable")]
        public string Visable { get; set; }
    }

    public class DisplayProperties
    {
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
        [XmlAttribute(AttributeName = "useLayer")]
        public string UseLayer { get; set; }
        [XmlAttribute(AttributeName = "color")]
        public string Color { get; set; }
        [XmlAttribute(AttributeName = "useColor")]
        public string UseColor { get; set; }
        [XmlAttribute(AttributeName = "lineType")]
        public string LineType { get; set; }
        [XmlAttribute(AttributeName = "useLineType")]
        public string UseLineType { get; set; }
        [XmlAttribute(AttributeName = "lineWeight")]
        public string LineWeight { get; set; }
        [XmlAttribute(AttributeName = "useLineWeight")]
        public string UseLineWeight { get; set; }
    }
}
