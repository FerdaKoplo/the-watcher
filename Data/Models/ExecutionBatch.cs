using System.ComponentModel.DataAnnotations;
namespace TheWatcher.Data.Models
{
    public class ExecutionBatch
    {
        [Key]
        public int Id { get; set; }

        public DateTime RunDate { get; set; } = DateTime.Now;

        public string TriggeredByRuleName { get; set; }
        public int TotalFilesProcessed { get; private set; }

        public int TotalErrors { get; set; }

        public List<FileExecutionLog> Logs { get; set; } = new();
        public void AddLog(FileExecutionLog log)
        {
            Logs.Add(log);
            TotalFilesProcessed++;

            if (log.Status == LogStatus.Failed)
            {
                TotalErrors++;
            }
        }
    }
}
