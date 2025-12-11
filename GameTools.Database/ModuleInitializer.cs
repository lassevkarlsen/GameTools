using LVK.Bootstrapping;

using Microsoft.EntityFrameworkCore;

namespace GameTools.Database;

internal class ModuleInitializer : IModuleInitializer
{
    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;

    public ModuleInitializer(IDbContextFactory<GameToolsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}