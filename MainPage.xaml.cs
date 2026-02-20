using System.Windows.Input;
using System.Diagnostics;

namespace TheWatcher
{
    public partial class MainPage : ContentPage
    {
        public ICommand ShowAppCommand { get; }
        public ICommand ExitAppCommand { get; }

        public MainPage()
        {
            InitializeComponent();
            ShowAppCommand = new Command(() => ShowApp_Clicked(this, EventArgs.Empty));
            ExitAppCommand = new Command(() => ExitApp_Clicked(this, EventArgs.Empty));
            
            BindingContext = this;
            Debug.WriteLine("=== [LOG] MainPage Initialized and Command Bound ===");
        }

        private void ShowApp_Clicked(object? sender, EventArgs e)
        {
            Debug.WriteLine("=== [LOG] ShowApp_Clicked TRIGGERED! ===");
            if (Application.Current is App app)
            {
                Debug.WriteLine("=== [LOG] Calling app.BringToFront()... ===");
                app.BringToFront();
            }
            else
            {
                Debug.WriteLine("=== [ERROR] Application.Current is not App! ===");
            }
        }

        private void ExitApp_Clicked(object? sender, EventArgs e)
        {
            Debug.WriteLine("=== [LOG] ExitApp_Clicked TRIGGERED! ===");
            if (Application.Current is App app)
            {
                Debug.WriteLine("=== [LOG] Calling app.ForceQuit()... ===");
                app.ForceQuit();
            }
        }
    }
}
