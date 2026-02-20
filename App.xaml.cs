using TheWatcher.Data;
using System.Diagnostics;
using TheWatcher.Workers;

namespace TheWatcher
{
    public partial class App : Application
    {
#if WINDOWS
    private Microsoft.UI.Windowing.AppWindow? _appWindow;
#endif
        public App(OrchestratorWorker worker, AppDbContext db)
        {
            InitializeComponent();
            MainPage = new MainPage();
            db.Database.EnsureCreated();

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "watcher.db");

            System.Diagnostics.Debug.WriteLine($"DB PATH: {dbPath}");

            _ = worker.StartAsync(CancellationToken.None);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);
            //var window = new Window(new MainPage()) { Title = "The Watcher" };

            window.Created += (s, e) =>
            {
#if WINDOWS
                Debug.WriteLine("=== [LOG] Window.Created Event Fired ===");
                var nativeWindow = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
                var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            
                _appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                Debug.WriteLine("=== [LOG] _appWindow successfully captured ===");

                _appWindow.Closing += (sender, args) =>
                {
                    Debug.WriteLine("=== [LOG] 'X' Button Clicked - Hiding Window ===");
                    args.Cancel = true;
                    _appWindow.Hide();
                };
#endif
            };

            return window;
            //return new Window(new MainPage()) { Title = "TheWatcher" };
        }

        public void BringToFront()
        {
            Debug.WriteLine("=== [LOG] BringToFront() Called ===");
#if WINDOWS
            if (_appWindow != null)
            {
                Debug.WriteLine("=== [LOG] _appWindow found! Attempting to Show()... ===");
                _appWindow.Show();
            }
            else
            {
                Debug.WriteLine("=== [ERROR] _appWindow is NULL! Falling back to dynamic search... ===");
                var window = Application.Current?.Windows.FirstOrDefault();
            
                if (window == null) Debug.WriteLine("=== [ERROR] Dynamic Window is NULL! ===");
            
                if (window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
            {
                Debug.WriteLine("=== [LOG] Dynamic Window found. Showing... ===");
                var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                appWindow.Show();
            }
            else
            {
                Debug.WriteLine("=== [ERROR] nativeWindow cast failed! ===");
            }
        }
#endif
        }

        public void ForceQuit()
        {
            Debug.WriteLine("=== [LOG] ForceQuit() Called - Sledgehammer active ===");
            Application.Current?.Quit();
            Environment.Exit(0);
        }
    }
}
