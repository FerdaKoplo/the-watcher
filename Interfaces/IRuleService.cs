using System;
using System.Collections.Generic;
using System.Text;
using TheWatcher.Data.Models;

namespace TheWatcher.Interfaces
{
    public interface IRuleService
    {
        Task<List<OrganizerRuler>> GetAllRulesAsync();
        Task<OrganizerRuler?> GetRuleByIdAsync(int id);
        Task SaveRuleAsync(OrganizerRuler rule);
        Task DeleteRuleAsync(int id);
    }
}
