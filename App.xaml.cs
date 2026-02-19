using TheWatcher.Data;
using TheWatcher.Workers;

namespace TheWatcher
{
    public partial class App : Application
    {
        public App(OrchestratorWorker worker, AppDbContext db)
        {
            InitializeComponent();
            //MainPage = new MainPage();
            db.Database.EnsureCreated();

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "watcher.db");

            System.Diagnostics.Debug.WriteLine($"DB PATH: {dbPath}");

            _ = worker.StartAsync(CancellationToken.None);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "TheWatcher" };
        }
    }
}
