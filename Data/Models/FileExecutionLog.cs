using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace TheWatcher.Data.Models
{
    public class FileExecutionLog
    {
        [Key]
        public int Id { get; set; }


        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string RuleName { get; set; } = string.Empty;
        public string ActionTaken {  get; set; } = string.Empty;


        public string OriginalFilePath { get; set; } = string.Empty;
        public string? NewFilePath { get; set; } = string.Empty;

        public LogStatus Status {  get; set; }  
        public string? ErrorMessage { get; set; } 
        public long FileByteSize {  get; set; }

        public int ExecutionBatchId { get; set; } 
        public ExecutionBatch Batch { get; set; }

       
    }
     public enum LogStatus
        {
            Success,
            Failed,
            Skipped,
            Warning,
            DryRun
        }
}
