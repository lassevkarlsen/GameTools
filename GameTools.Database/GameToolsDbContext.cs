using Microsoft.EntityFrameworkCore;

namespace GameTools.Database;

public class GameToolsDbContext : DbContext
{
    public GameToolsDbContext(DbContextOptions<GameToolsDbContext> options)
        : base(options)
    {

    }

    public async Task EnsureProfileExistAsync(Guid profileId)
    {
        Profile? existingProfile = await Profiles.FirstOrDefaultAsync(p => p.Id == profileId);
        if (existingProfile != null)
        {
            return;
        }

        var newProfile = new Profile { Id = profileId };
        Profiles.Add(newProfile);
        await SaveChangesAsync();
    }

    public DbSet<Profile> Profiles { get; set; }
    public DbSet<GameTimer> GameTimers { get; set; }

    public DbSet<ShoppingListCategory> ShoppingListCategories { get; set; }
    public DbSet<ShoppingListItem> ShoppingListItems { get; set; }

    public DbSet<NoMansSkyGuildSystemReward> NoMansSkyGuildSystemRewards { get; set; }
    public DbSet<NoMansSkyGuildSystem> NoMansSkyGuildSystems { get; set; }

    public DbSet<NoMansSkyPortalAddress> NoMansSkyPortalAddresses { get; set; }
    public DbSet<NoMansSkyGalaxy> NoMansSkyGalaxies { get; set; }
}