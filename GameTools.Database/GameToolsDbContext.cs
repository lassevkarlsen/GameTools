using Microsoft.EntityFrameworkCore;

namespace GameTools.Database;

public class GameToolsDbContext : DbContext
{
    public GameToolsDbContext(DbContextOptions<GameToolsDbContext> options)
        : base(options)
    {

    }

    public DbSet<ProfileSettings> ProfileSettings { get; set; }
}