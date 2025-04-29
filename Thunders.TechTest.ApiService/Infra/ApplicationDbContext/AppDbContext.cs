using Microsoft.EntityFrameworkCore;
using Thunders.TechTest.ApiService.Domain.Entities;

namespace Thunders.TechTest.ApiService.Infra.ApplicationDbContext;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<TollUsage>(t =>
        {
            t.HasKey(e => e.Id);

            t.Property(e => e.TollDescription).HasMaxLength(200);
            t.Property(e => e.City).HasMaxLength(150);
            t.Property(e => e.State).HasMaxLength(100);
        });

        base.OnModelCreating(builder);
    }

    public DbSet<TollUsage> TollUsages { get; set; }
}
