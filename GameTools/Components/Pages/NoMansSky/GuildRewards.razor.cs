using GameTools.Database;

using LVK.Events;

using Microsoft.EntityFrameworkCore;

using Radzen;

namespace GameTools.Components.Pages.NoMansSky;

public partial class GuildRewards
{
    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;
    private readonly IEventBus _eventBus;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly DialogService _dialogService;
    private readonly ILogger<Timers> _logger;
    private NewGuildSystemModel _newSystemModel = new();
    private List<GuildSystem> _systems = [];

    public GuildRewards(IDbContextFactory<GameToolsDbContext> dbContextFactory, IEventBus eventBus,
        IHostApplicationLifetime hostApplicationLifetime, DialogService dialogService, ILogger<Timers> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _hostApplicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await ReloadSystems();

        // _ = PeriodicRefresh();
    }

    private async Task ReloadSystems()
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.EnsureProfileExistAsync(ProfileId!.Value);
        _systems = await dbContext.NoMansSkyGuildSystems.Where(gt => gt.ProfileId == ProfileId!.Value)
           .Include(t => t.Profile)
           .Include(t => t.Rewards)
           .OrderBy(t => t.Name)
           .ToListAsync();
    }

    private async Task AddSystem()
    {
        _newSystemModel.ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(_newSystemModel.SystemName) && string.IsNullOrWhiteSpace(_newSystemModel.GuildName))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_newSystemModel.SystemName) || string.IsNullOrWhiteSpace(_newSystemModel.GuildName))
        {
            _newSystemModel.ErrorMessage = "Need both system and guild name to add";
            return;
        }

        if (_systems.Any(gt => gt.Name == _newSystemModel.SystemName))
        {
            _newSystemModel.ErrorMessage = "A system with that name already exists";
            return;
        }

        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();

        dbContext.NoMansSkyGuildSystems.Add(new GuildSystem
        {
            ProfileId = ProfileId!.Value,
            Name = _newSystemModel.SystemName,
            Guild = _newSystemModel.GuildName,
        });
        await dbContext.SaveChangesAsync();
        await ReloadSystems();

        _newSystemModel.Clear();
    }

    private Task OnCheckRedeemCheckbox(GuildSystemReward reward, bool value)
    {
        return Task.CompletedTask;
    }

    private Task OnAddRewardClicked(GuildSystem system)
    {
        return Task.CompletedTask;
        // throw new NotImplementedException();
    }
}