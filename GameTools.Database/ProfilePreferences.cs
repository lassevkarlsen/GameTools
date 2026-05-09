using Microsoft.EntityFrameworkCore;

namespace GameTools.Database;

public class ProfilePreferences : IProfilePreferences
{
    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;

    public ProfilePreferences(IDbContextFactory<GameToolsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task<string> GetPreference(Guid profileId, string key)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();

        string? value = await dbContext.ProfilePreferences
            .AsNoTracking()
            .Where(p => p.ProfileId == profileId && p.Key == key)
            .Select(p => p.Value)
            .FirstOrDefaultAsync();

        return value ?? string.Empty;
    }

    public async Task SetPreference(Guid profileId, string key, string value)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();

        ProfilePreference? existingPreference = await dbContext.ProfilePreferences
            .FirstOrDefaultAsync(p => p.ProfileId == profileId && p.Key == key);

        if (existingPreference is null)
        {
            await dbContext.EnsureProfileExistAsync(profileId);
            dbContext.ProfilePreferences.Add(new ProfilePreference
            {
                ProfileId = profileId,
                Key = key,
                Value = value,
            });
        }
        else
        {
            existingPreference.Value = value;
        }

        await dbContext.SaveChangesAsync();
    }
}