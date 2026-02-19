using System.ComponentModel.DataAnnotations;

namespace TheWatcher.Data.Models
{
    public class RuleCondition
    {
        [Key]
        public int Id { get; set; }
        public int OrganizedRuleId { get; set; }
        public OrganizerRuler OrganizeRule { get; set; }

        public ConditionType Field { get; set; }
        public OperatorType Operator { get; set; }

        public string Value { get; set; }

        public enum OperatorType
        {
            Equals,
            NotEquals,
            StartsWith,
            EndsWith,
            Contains,
            GreaterThan,
            LessThan
        }
        public enum ConditionType
        {
            Extension,
            Size,
            DateCreated,
            FileName
        }
    }
}
