using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
namespace TheWatcher.Data.Models
{
    public class ExecutionBatch
    {
        [Key]
        public int Id { get; set; }

        public DateTime RunDate { get; set; } = DateTime.Now;

        public string TriggeredByRuleName { get; set; }
        public int TotalFilesProcessed { get; set; }

        public int TotalErrors { get; set; }

        public List<FileExecutionLog> Logs { get; set; } = new();
    }
}
