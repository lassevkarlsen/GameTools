using GameTools.Database;

using Microsoft.EntityFrameworkCore;

using Radzen;

namespace GameTools.Components.Pages;

public partial class ProfileConfiguration
{
    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;

    public ProfileConfiguration(IDbContextFactory<GameToolsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        SetPageTitle?.Invoke("Profile");
        await LoadProfileSettings();
    }

    private async Task LoadProfileSettings()
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        ProfileSettings? settings = await dbContext.ProfileSettings.FirstOrDefaultAsync(pf => pf.ProfileId == UserId);
        if (settings != null)
        {
        }
    }
}