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
        public string Uuid { get; set; }
        public string Desc { get; set; }
    }
}
