using Microsoft.EntityFrameworkCore;
using Thunders.TechTest.ApiService.Domain.Entities;

namespace Thunders.TechTest.ApiService.Infra.ApplicationDbContext;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ParticipantVoting>()
            .HasKey(pv => new { pv.Id, pv.VotingId, pv.ParticipantId });

        builder.Entity<TollUsage>(t =>
        {
            t.HasKey(e => e.Id);

            t.Property();
        });
            

        base.OnModelCreating(builder);
    }

    public DbSet<TollUsage> TollUsages { get; set; }
}
