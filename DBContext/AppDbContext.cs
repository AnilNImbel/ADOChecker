using Microsoft.EntityFrameworkCore;
using ADOAnalyser.Models;

namespace ADOAnalyser.DBContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TestRunResult> TestRunResults { get; set; }
        public DbSet<TestRunDetail> TestRunDetails { get; set; }
        public DbSet<TestRunDetail> EmailConfig { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestRunDetail>()
                .HasOne<TestRunResult>()
                .WithMany()
                .HasForeignKey(d => d.RunId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
