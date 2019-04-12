using System.Xml.Serialization;

namespace ShortcutMonitor.GUI.Xml
{
    /// <summary>
    /// Файл ShortcutsHistory.xml
    /// </summary>
    public class History
    {
        public ShortProjectID ShortProjectID { get; set; }
    }

    public class ShortProjectID
    {
        [XmlAttribute(AttributeName = "uuid")]
        public string Uuid { get; set; }
        [XmlAttribute(AttributeName = "desc")]
        public string Desc { get; set; }
    }
}
