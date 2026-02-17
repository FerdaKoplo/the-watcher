using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TheWatcher.Data;
using TheWatcher.Data.Models;
using TheWatcher.Interfaces;

namespace TheWatcher.Services
{
    public class RuleService : IRuleService
    {
        private readonly AppDbContext _context;
        public RuleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrganizerRuler>> GetAllRulesAsync()
        {
            return await _context.OrganizerRules
                .Include(r => r.Conditions)
                .ToListAsync();
        }

        public async Task<OrganizerRuler?> GetRuleByIdAsync(int id)
        {
            return await _context.OrganizerRules
                .Include(r => r.Conditions)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task SaveRuleAsync(OrganizerRuler rule)
        {
            if (rule.Id == 0)
            {
                _context.OrganizerRules.Add(rule);
            }
            else
            {
                _context.OrganizerRules.Update(rule);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRuleAsync(int id)
        {
            var rule = await _context.OrganizerRules.FindAsync(id);
            if (rule != null)
            {
                _context.OrganizerRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }
    }
}
