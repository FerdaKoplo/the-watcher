using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TheWatcher.Data;
using TheWatcher.Data.Models;
using TheWatcher.Interfaces;
using TheWatcher.Services;

namespace TheWatcher.Workers
{
    public class OrchestratorWorker : BackgroundService
    {
        private readonly ILogger<OrchestratorWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly FileWatcherService _watcherService;
        private readonly IServiceScopeFactory _scopeFactory;

        private List<OrganizerRuler> _activeRules = new();
        public OrchestratorWorker(
            ILogger<OrchestratorWorker> logger,
            IServiceProvider serviceProvider,
            FileWatcherService watcherService,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _watcherService = watcherService;
            _scopeFactory = scopeFactory;
        }

        private async Task ReloadRulesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                _activeRules = await db.OrganizerRules
                    .Include(r => r.Conditions)
                    .Where(r => r.Status == RuleStatus.Active)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation($"Reloaded {_activeRules.Count} rules from Database.");
            }

            _watcherService.StopAll();
            _watcherService.StartWatching(_activeRules);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Orchestrator Service is starting...");
            await ReloadRulesAsync();
            //_activeRules = GetMockRules(); 
            foreach (var rule in _activeRules)
            {
                if (!Directory.Exists(rule.SourceDirectory))
                {
                    Directory.CreateDirectory(rule.SourceDirectory);
                    _logger.LogInformation($"Created missing source folder: {rule.SourceDirectory}");
                }

                if (!string.IsNullOrEmpty(rule.DestinationDirectory) && !Directory.Exists(rule.DestinationDirectory))
                {
                    Directory.CreateDirectory(rule.DestinationDirectory);
                }
            }
            _logger.LogInformation($"Loaded {_activeRules.Count} active rules.");
            _watcherService.OnFileDetected += async (task) => await ProcessDetectedFile(task);

            _watcherService.StartWatching(_activeRules);
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Orchestrator Service is stopping.");
            _watcherService.StopAll();
        }


        private async Task ProcessDetectedFile(FileTask task)
        {
            _logger.LogInformation($"[DETECTED] {task.FileName} at {task.OriginalFullPath}");

            using (var scope = _serviceProvider.CreateScope())
            {
                var engine = scope.ServiceProvider.GetRequiredService<IOrganizerEngine>();
                task = engine.ProcessFile(task, _activeRules);

                if (task.MatchedRule != null)
                {
                    _logger.LogInformation($"   -> Matched Rule: {task.MatchedRule.Name}");
                    _logger.LogInformation($"   -> Action: {task.MatchedRule.ActionType}");

                    var log = await engine.ExecuteTaskAsync(task);

                    if (log.Status == LogStatus.Success)
                    {
                        _logger.LogInformation($"   -> [SUCCESS] Moved to: {log.NewFilePath}");
                    }
                    else if (log.Status == LogStatus.Skipped)
                    {
                        _logger.LogWarning($"   -> [SKIPPED] {log.ErrorMessage}");
                    }
                    else
                    {
                        _logger.LogError($"   -> [FAILED] {log.ErrorMessage}");
                    }

                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var dbRecord = new FileExecutionLog
                    {
                        RuleName = task.MatchedRule.Name,
                        ActionTaken = task.MatchedRule.ActionType.ToString(),
                        OriginalFilePath = task.OriginalFullPath,
                        NewFilePath = log.NewFilePath,
                        Status = log.Status,
                        Timestamp = DateTime.Now,
                        ErrorMessage = log.ErrorMessage
                    };

                    dbContext.FileExecutionLogs.Add(dbRecord);
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    _logger.LogInformation("   -> No rule matched. Ignoring.");
                }
            }
        }


        //private List<OrganizerRuler> GetMockRules()
        //{
        //    return new List<OrganizerRuler>
        //    {
        //        new OrganizerRuler
        //        {
        //            Id = 1,
        //            Name = "Move Text Files",
        //            SourceDirectory = @"C:\WatcherTest\Input",
        //            DestinationDirectory = @"C:\WatcherTest\TextFiles",
        //            ActionType = RuleActionType.Move,
        //            Conditions = new List<RuleCondition>
        //            {
        //                new RuleCondition { Field = RuleCondition.ConditionType.Extension, Value = ".txt", Operator = RuleCondition.OperatorType.Equals }
        //            }
        //        },
        //        new OrganizerRuler
        //        {
        //            Id = 2,
        //            Name = "Archive PDFs",
        //            SourceDirectory = @"C:\WatcherTest\Input",
        //            DestinationDirectory = @"C:\WatcherTest\Archives",
        //            ActionType = RuleActionType.Archive,
        //            Conditions = new List<RuleCondition>
        //            {
        //                new RuleCondition { Field = RuleCondition.ConditionType.Extension, Value = ".pdf", Operator = RuleCondition.OperatorType.Equals }
        //            }
        //        }
        //    };
        //}
    }
}
