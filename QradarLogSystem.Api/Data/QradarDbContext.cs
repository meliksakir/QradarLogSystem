using Microsoft.EntityFrameworkCore;
using QradarLogSystem.Api.Data.Entities;

namespace QradarLogSystem.Api.Data
{
    public class QradarDbContext : DbContext
    {
        public QradarDbContext(
            DbContextOptions<QradarDbContext> options)
            : base(options)
        {
        }

        public DbSet<EventEntity> Events { get; set; }

        public DbSet<ProcessingRunEntity> ProcessingRuns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventEntity>()
                .ToTable("Events");

            modelBuilder.Entity<ProcessingRunEntity>()
                .ToTable("ProcessingRuns");

            base.OnModelCreating(modelBuilder);
        }
    }
}
