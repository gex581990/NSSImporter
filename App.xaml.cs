using Microsoft.UI.Xaml;

namespace NSSImporter
{
    public partial class App : Application
    {
        public static Window? MainWindow { get; private set; }
        private Window? _window;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new Views.MainWindow();
            MainWindow = _window;
            _window.Activate();
        }
    }
}
