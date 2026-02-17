using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace TheWatcher.Data.Models
{
    public class OrganizerRuler
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string SourceDirectory { get; set; } = string.Empty;

        public string? FileExtension {  get; set; }

        public int MaxRecursionDepth { get; set; } = 0;

        public string? FilterNameContain { get; set; }
        public long? MinByteSize {  get; set; }
        public RuleActionType ActionType { get; set; }
        
        public string? DestinationDirectory {  get; set; }
        public string? RenamePattern { get; set; }
        public RuleStatus Status { get; set; } = RuleStatus.Active;
        public List<RuleCondition> Conditions { get; set; }
    }
        public enum RuleStatus
        {
            Draft,
            Active,
            Paused,
            Disabled,
            Archived
        }

        public enum RuleActionType 
        { 
            Move, 
            Copy, 
            Rename,
            Delete,
            Archive
        }
}
