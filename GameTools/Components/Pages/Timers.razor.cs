using GameTools.Database;
using GameTools.Workers.Events;

using Humanizer;

using LVK.Events;

using Microsoft.EntityFrameworkCore;

using Radzen;

namespace GameTools.Components.Pages;

public partial class Timers : IAsyncDisposable
{
    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;
    private readonly IEventBus _eventBus;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly DialogService _dialogService;
    private readonly ILogger<Timers> _logger;

    private List<GameTimer> _timers = [];

    private readonly NewTimerModel _newTimerModel = new();

    private bool _refreshingStarted;

    private IDisposable? _subscription;

    public Timers(IDbContextFactory<GameToolsDbContext> dbContextFactory, IEventBus eventBus,
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
        await ReloadTimers();

        _ = PeriodicRefresh();
        _subscription ??= _eventBus.Subscribe<TimersEditedForProfileEvent>(OnTimersEditedForProfile);

        SetPageTitle?.Invoke("General Tools :: Timers");
    }

    private async Task OnTimersEditedForProfile(TimersEditedForProfileEvent evt)
    {
        if (evt.ProfileId == ProfileId!.Value)
        {
            _logger.LogInformation("Timers edited for profile {ProfileId}", evt.ProfileId);
            await InvokeAsync(async () =>
            {
                await ReloadTimers();
                StateHasChanged();
            });
        }
    }

    private async Task PeriodicRefresh()
    {
        if (_refreshingStarted)
        {
            return;
        }

        _refreshingStarted = true;
        while (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
        {
            await Task.Delay(1000);
            if (_timers.Any(t => t.ElapsesAt != null))
            {
                StateHasChanged();
            }
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
        await _eventBus.PublishAsync(new TimersEditedForProfileEvent
        {
            ProfileId = ProfileId!.Value,
        });

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
            await _eventBus.PublishAsync(new TimersEditedForProfileEvent
            {
                ProfileId = ProfileId!.Value,
            });
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
            await _eventBus.PublishAsync(new TimersEditedForProfileEvent
            {
                ProfileId = ProfileId!.Value,
            });
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
            await _eventBus.PublishAsync(new TimersEditedForProfileEvent
            {
                ProfileId = ProfileId!.Value,
            });
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
                await _eventBus.PublishAsync(new TimersEditedForProfileEvent
                {
                    ProfileId = ProfileId!.Value,
                });
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
        await _eventBus.PublishAsync(new TimersUpdatedEvent());
        await _eventBus.PublishAsync(new TimersEditedForProfileEvent
        {
            ProfileId = ProfileId!.Value,
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_subscription is IAsyncDisposable subscriptionAsyncDisposable)
        {
            await subscriptionAsyncDisposable.DisposeAsync();
        }
        else if (_subscription != null)
        {
            _subscription.Dispose();
        }
    }
}