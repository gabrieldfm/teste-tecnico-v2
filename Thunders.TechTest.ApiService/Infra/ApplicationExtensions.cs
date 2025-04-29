using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Thunders.TechTest.ApiService.Infra.ApplicationDbContext;

namespace Thunders.TechTest.ApiService.Infra;

public static class ApplicationExtensions
{
    public static async Task ConfigureDataBaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await EnsureDataBaseIsCreatedAsync(dbContext);
        await RunMigrationsAsync(dbContext);
    }

    private static async Task EnsureDataBaseIsCreatedAsync(AppDbContext dbContext)
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            if (!await dbCreator.ExistsAsync())
                await dbCreator.CreateAsync();
        });
    }

    private static async Task RunMigrationsAsync(AppDbContext dbContext)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            await dbContext.Database.MigrateAsync();
            await transaction.CommitAsync();
;        });
    }
}
