using GameTools.Database;
using GameTools.Workers.Events;

using Humanizer;

using LVK.Events;

using Microsoft.EntityFrameworkCore;

using Radzen;

namespace GameTools.Components.Pages;

public partial class Timers
{
    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;
    private readonly IEventBus _eventBus;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly DialogService _dialogService;

    private List<GameTimer> _timers = [];

    private readonly NewTimerModel _newTimerModel = new();

    private readonly PeriodicTimer _refreshTimer = new(TimeSpan.FromSeconds(1));
    private bool _refreshingStarted;

    public Timers(IDbContextFactory<GameToolsDbContext> dbContextFactory, IEventBus eventBus,
            IHostApplicationLifetime hostApplicationLifetime, DialogService dialogService)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _hostApplicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await ReloadTimers();

        _ = PeriodicRefresh();
    }

    private async Task PeriodicRefresh()
    {
        if (_refreshingStarted)
        {
            return;
        }

        _refreshingStarted = true;
        while (await _refreshTimer.WaitForNextTickAsync() && !_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
        {
            if (!_timers.Any(t => t.ElapsesAt != null))
            {
                continue;
            }
            await ReloadTimers();
            StateHasChanged();
        }
    }

    private async Task ReloadTimers()
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.EnsureProfileExistAsync(ProfileId!.Value);
        _timers = await dbContext.GameTimers.Where(gt => gt.ProfileId == ProfileId!.Value)
           .Include(t => t.Profile)
           .OrderBy(t => t.Name)
           .ToListAsync();
    }

    private async Task AddTimer()
    {
        if (!_newTimerModel.TryParse(out TimeSpan duration, out string name))
        {
            return;
        }

        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.EnsureProfileExistAsync(ProfileId!.Value);

        if (name == "")
        {
            name = duration.Humanize();
        }

        dbContext.GameTimers.Add(new GameTimer
        {
            ProfileId = ProfileId!.Value,
            Duration = duration,
            ElapsesAt = DateTimeOffset.UtcNow + duration,
            Name = name,
        });
        await dbContext.SaveChangesAsync();
        await _eventBus.PublishAsync(new TimersUpdatedEvent());
        await ReloadTimers();

        _newTimerModel.Clear();
    }

    private async Task StartOrResumeTimer(GameTimer timer)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        GameTimer? foundTimer = await dbContext.GameTimers.FirstOrDefaultAsync(t => t.Id == timer.Id);
        if (foundTimer != null)
        {
            if (foundTimer.Remaining != null)
            {
                foundTimer.ElapsesAt = DateTimeOffset.UtcNow + foundTimer.Remaining;
                foundTimer.Remaining = null;
            }
            else
            {
                foundTimer.ElapsesAt = DateTimeOffset.UtcNow + foundTimer.Duration;
            }

            foundTimer.CompletionProcessed = false;

            await dbContext.SaveChangesAsync();
            await _eventBus.PublishAsync(new TimersUpdatedEvent());
            await ReloadTimers();
        }
    }

    private async Task PauseTimer(GameTimer timer)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        GameTimer? foundTimer = await dbContext.GameTimers.FirstOrDefaultAsync(t => t.Id == timer.Id);
        if (foundTimer != null)
        {
            foundTimer.Remaining = foundTimer.ElapsesAt - DateTimeOffset.UtcNow;
            foundTimer.ElapsesAt = null;
            await dbContext.SaveChangesAsync();
            await _eventBus.PublishAsync(new TimersUpdatedEvent());
            await ReloadTimers();
        }
    }

    private async Task StopTimer(GameTimer timer)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        GameTimer? foundTimer = await dbContext.GameTimers.FirstOrDefaultAsync(t => t.Id == timer.Id);
        if (foundTimer != null)
        {
            foundTimer.Remaining = null;
            foundTimer.ElapsesAt = null;
            foundTimer.CompletionProcessed = false;
            await dbContext.SaveChangesAsync();
            await _eventBus.PublishAsync(new TimersUpdatedEvent());
            await ReloadTimers();
        }
    }

    private async Task DeleteTimer(GameTimer timer)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        GameTimer? foundTimer = await dbContext.GameTimers.FirstOrDefaultAsync(t => t.Id == timer.Id);
        if (foundTimer != null)
        {
            bool? response = await _dialogService.Confirm($"Do you want to delete timer '{foundTimer.Name}'?", "Delete timer?", new()
            {
                OkButtonText = "Delete",
                Icon = "warning",
            });

            if (response ?? false)
            {
                dbContext.GameTimers.Remove(foundTimer);
                await dbContext.SaveChangesAsync();
                await _eventBus.PublishAsync(new TimersUpdatedEvent());
                await ReloadTimers();
            }
        }
    }

    private async Task PauseAllTimers()
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.EnsureProfileExistAsync(ProfileId!.Value);
        List<GameTimer> timers = await dbContext.GameTimers.Where(gt => gt.ProfileId == ProfileId!.Value)
           .Include(t => t.Profile)
           .OrderBy(t => t.Name)
           .ToListAsync();

        foreach (GameTimer timer in timers)
        {
            if (timer.ElapsesAt != null && timer.ElapsesAt >= DateTimeOffset.UtcNow)
            {
                timer.Remaining = timer.ElapsesAt - DateTimeOffset.UtcNow;
                timer.ElapsesAt = null;
            }
        }

        await dbContext.SaveChangesAsync();
        await ReloadTimers();
    }
}