using Microsoft.EntityFrameworkCore;
using TheWatcher.Data.Models;

namespace TheWatcher.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<OrganizerRuler> OrganizerRules { get; set; }
        public DbSet<RuleCondition> RuleConditions { get; set; }
        public DbSet<ExecutionBatch> ExecutionBatches { get; set; }
        public DbSet<FileExecutionLog> FileExecutionLogs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrganizerRuler>()
                .HasMany(r => r.Conditions)
                .WithOne(c => c.OrganizeRule)
                .HasForeignKey(c => c.OrganizedRuleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExecutionBatch>()
                .HasMany(b => b.Logs)
                .WithOne(l => l.Batch)
                .HasForeignKey(l => l.ExecutionBatchId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}