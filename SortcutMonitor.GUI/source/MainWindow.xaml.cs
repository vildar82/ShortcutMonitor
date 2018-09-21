namespace SortcutMonitor.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
            : base(new MainVM())
        {
            InitializeComponent();
        }
    }
}
