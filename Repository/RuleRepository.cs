using Microsoft.EntityFrameworkCore;
using TheWatcher.Data;
using TheWatcher.Data.Models;
using TheWatcher.Interfaces;

namespace TheWatcher.Repository
{
    public class RuleRepository : IRuleRepository
    {
        private readonly AppDbContext _context;

        public RuleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrganizerRuler>> GetActiveRulesAsync()
        {
            return await _context.OrganizerRules
                .Include(r => r.Conditions)
                .Where(r => r.Status == RuleStatus.Active)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddRuleAsync(OrganizerRuler rule)
        {
            _context.OrganizerRules.Add(rule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRuleAsync(int ruleId)
        {
            var rule = await _context.OrganizerRules.FindAsync(ruleId);
            if (rule != null)
            {
                _context.OrganizerRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveExecutionLogAsync(FileExecutionLog log)
        {
            _context.FileExecutionLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
