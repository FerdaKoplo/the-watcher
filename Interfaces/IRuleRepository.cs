using TheWatcher.Data.Models;

namespace TheWatcher.Interfaces
{
    public interface IRuleRepository
    {
        Task<List<OrganizerRuler>> GetActiveRulesAsync();
        Task AddRuleAsync(OrganizerRuler rule);
        Task DeleteRuleAsync(int ruleId);
        Task SaveExecutionLogAsync(FileExecutionLog log);

    }
}
