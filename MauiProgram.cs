using H.NotifyIcon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheWatcher.Data;
using TheWatcher.Interfaces;
using TheWatcher.Repository;
using TheWatcher.Services;
using TheWatcher.Workers;

namespace TheWatcher
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseNotifyIcon()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddScoped<IRuleService, RuleService>();
            builder.Services.AddSingleton<FileWatcherService>();
            builder.Services.AddScoped<IOrganizerEngine, OrganizerEngineService>();

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "watcher.db");

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite($"Data Source={dbPath}");
            });

            builder.Services.AddSingleton<OrchestratorWorker>();

            builder.Services.AddSingleton<IFolderPickerService, FolderPickerService>();


            // repository
            builder.Services.AddScoped<IRuleRepository, RuleRepository>();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
