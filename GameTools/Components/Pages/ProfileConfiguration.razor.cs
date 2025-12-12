using GameTools.Database;

using Microsoft.EntityFrameworkCore;

using Radzen;

namespace GameTools.Components.Pages;

public partial class ProfileConfiguration
{
    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;
    private readonly NotificationService _notificationService;
    private ProfileConfigurationModel? _model;

    public ProfileConfiguration(IDbContextFactory<GameToolsDbContext> dbContextFactory, NotificationService notificationService)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
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
        Profile? profile = await dbContext.Profiles.FirstOrDefaultAsync(pf => pf.Id == ProfileId);
        if (profile != null)
        {
            _model = new ProfileConfigurationModel
            {
                Name = profile.Name == Profile.DefaultProfileName ? "" : profile.Name, PushoverUserKey = profile.PushoverUserKey ?? "",
            };
        }
    }

    private async Task Save()
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        Profile? profile = await dbContext.Profiles.FirstOrDefaultAsync(pf => pf.Id == ProfileId);
        if (profile != null)
        {
            if (string.IsNullOrWhiteSpace(_model!.Name))
            {
                profile.Name = Profile.DefaultProfileName;
            }
            else
            {
                profile.Name = _model!.Name;
            }

            profile.PushoverUserKey = _model.PushoverUserKey;
            await dbContext.SaveChangesAsync();

            _notificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Profile saved",
                Detail = $"Profile '{profile.Name}' saved",
                Duration = 10_000,
                ShowProgress = true,
                Payload = Guid.NewGuid(),
            });
        }
    }
}