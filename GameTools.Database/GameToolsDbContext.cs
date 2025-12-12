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
}