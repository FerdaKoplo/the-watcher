using System;
using System.Collections.Generic;
using System.Text;

namespace TheWatcher.Data.Models
{
    public class FileTask
    {
        public string OriginalFullPath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long SizeBytez {  get; set; }

        public OrganizerRuler MatchedRule { get; set;  }

        public string ProposedNewPath { get; set; } = string.Empty;
        public string ProposedNewName { get; set; } = string.Empty;

        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        public string StatusMessage { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
        public DuplicatedState DuplicateState { get; set; } = DuplicatedState.Unknown; 
        public string? FileHash { get; set; }

    }
      public enum DuplicatedState
        {
            Unknown,
            Unique,
            Primary,
            Redundant
        }
        public enum TaskStatus
        {
            Pending,
            Ready,
            Processing,
            Completed,
            Failed
        }
}
