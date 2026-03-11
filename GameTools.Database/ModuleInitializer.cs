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
        await FillGalaxiesAsync(dbContext);
    }

    private async Task FillGalaxiesAsync(GameToolsDbContext dbContext)
    {
        var galaxies = new List<NoMansSkyGalaxy>
        {
            new() { Id = 1, Name = "Euclid"},
            new() { Id = 2, Name = "Hilbert Dimension"},
            new() { Id = 3, Name = "Calypso", Type = "Harsh" },
            new() { Id = 4, Name = "Hesperius Dimension"},
            new() { Id = 5, Name = "Hyades" },
            new() { Id = 6, Name = "Ickjamatew" },
            new() { Id = 7, Name = "Budullangr", Type = "Empty" },
            new() { Id = 8, Name = "Kikolgallr" },
            new() { Id = 9, Name = "Eltiensleen" },
            new() { Id = 10, Name = "Eissentam", Type = "Lush" },
        };

        List<NoMansSkyGalaxy> existing = await dbContext.NoMansSkyGalaxies.ToListAsync();
        foreach (NoMansSkyGalaxy galaxy in galaxies)
        {
            if (existing.Any(e => e.Id == galaxy.Id))
            {
                continue;
            }
            dbContext.NoMansSkyGalaxies.Add(galaxy);
        }

        await dbContext.SaveChangesAsync();
    }
}