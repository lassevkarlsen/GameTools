using GameTools.Components.Pages.Diablo4.Models;
using GameTools.Database;
using GameTools.Workers.Events;

using LVK.Events;

using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GameTools.Components.Pages.Diablo4;

public partial class Diablo4EventTimers : IAsyncDisposable
{
    protected override string LandingPageValue => LandingPageConstants.Values.Diablo4EventTimers;

    private const string _showLegionPreferenceKey = "d4-eventtimers-show-legion";
    private const string _showHelltidesPreferenceKey = "d4-eventtimers-show-helltides";
    private const string _showWorldBossesPreferenceKey = "d4-eventtimers-show-worldbosses";

    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;
    private readonly IEventBus _eventBus;
    private readonly IJSRuntime _jsRuntime;
    private readonly IProfilePreferences _profilePreferences;
    private readonly Lock _refreshLock = new();

    private readonly List<Diablo4Event> _events = [];
    private readonly List<Diablo4EventNotification> _notifications = [];
    private Profile? _profile;
    private readonly TimeSpan _refreshGracePeriod = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _refreshCheckInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _notificationEnableCutoff = TimeSpan.FromMinutes(5);

    private bool _countdownStarted;
    private bool _refreshLoopStarted;
    private bool _isDisposed;
    private bool _showLegionEvents = true;
    private bool _showHelltides = true;
    private bool _showWorldBosses = true;
    private DateTime _currentLocalTime = DateTime.Now;
    private CancellationTokenSource? _refreshLoopCancellationTokenSource;
    private Task? _refreshLoopTask;
    private IDisposable? _notificationsUpdatedSubscription;

    public Diablo4EventTimers(IDbContextFactory<GameToolsDbContext> dbContextFactory, IEventBus eventBus,
            IJSRuntime jsRuntime, IProfilePreferences profilePreferences)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _profilePreferences = profilePreferences ?? throw new ArgumentNullException(nameof(profilePreferences));
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _notificationsUpdatedSubscription = _eventBus.Subscribe<Diablo4EventNotificationsUpdatedEvent>(evt =>
        {
            if (_isDisposed || ProfileId is null || evt.ProfileId != ProfileId.Value)
            {
                return;
            }

            _ = InvokeAsync(async () =>
            {
                if (_isDisposed)
                {
                    return;
                }

                await LoadNotificationsAsync();
                StateHasChanged();
            });
        });

        await LoadEventTypePreferencesAsync();
        RefreshSchedule();
        await LoadProfileAsync();
        await LoadNotificationsAsync();
        StartRefreshLoop();

        SetPageTitle?.Invoke("Diablo 4 :: Event Timers");
    }

    private async Task LoadEventTypePreferencesAsync()
    {
        if (ProfileId is null)
        {
            return;
        }

        _showLegionEvents = await LoadEventTypePreferenceAsync(_showLegionPreferenceKey);
        _showHelltides = await LoadEventTypePreferenceAsync(_showHelltidesPreferenceKey);
        _showWorldBosses = await LoadEventTypePreferenceAsync(_showWorldBossesPreferenceKey);
    }

    private async Task<bool> LoadEventTypePreferenceAsync(string key)
    {
        if (ProfileId is null)
        {
            return true;
        }

        string value = await _profilePreferences.GetPreference(ProfileId.Value, key);
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return !bool.TryParse(value, out bool parsedValue) || parsedValue;
    }

    private async Task OnShowLegionEventsChanged(bool value)
    {
        _showLegionEvents = value;
        await PersistEventTypePreferenceAsync(_showLegionPreferenceKey, value);
        StateHasChanged();
    }

    private async Task OnShowHelltidesChanged(bool value)
    {
        _showHelltides = value;
        await PersistEventTypePreferenceAsync(_showHelltidesPreferenceKey, value);
        StateHasChanged();
    }

    private async Task OnShowWorldBossesChanged(bool value)
    {
        _showWorldBosses = value;
        await PersistEventTypePreferenceAsync(_showWorldBossesPreferenceKey, value);
        StateHasChanged();
    }

    private async Task PersistEventTypePreferenceAsync(string key, bool value)
    {
        if (ProfileId is null)
        {
            return;
        }

        await _profilePreferences.SetPreference(ProfileId.Value, key, value.ToString());
    }

    private async Task LoadProfileAsync()
    {
        if (ProfileId is null)
        {
            return;
        }

        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.EnsureProfileExistAsync(ProfileId.Value);
        _profile = await dbContext.Profiles.FirstOrDefaultAsync(profile => profile.Id == ProfileId.Value);
    }

    private async Task LoadNotificationsAsync()
    {
        if (ProfileId is null)
        {
            return;
        }

        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.EnsureProfileExistAsync(ProfileId.Value);

        List<Diablo4EventNotification> notifications = await dbContext.Diablo4EventNotifications
            .Where(notification => notification.ProfileId == ProfileId.Value)
            .OrderBy(notification => notification.OccursAt)
            .ToListAsync();

        lock (_refreshLock)
        {
            _notifications.Clear();
            _notifications.AddRange(notifications);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (_events.Count > 0)
        {
            await _jsRuntime.InvokeVoidAsync("gameTools.diablo4EventTimers.start", "#d4-event-timers");
            _countdownStarted = true;
        }
        else if (_countdownStarted)
        {
            await _jsRuntime.InvokeVoidAsync("gameTools.diablo4EventTimers.stop");
            _countdownStarted = false;
        }
    }

    private void RefreshSchedule()
    {
        List<Diablo4Event> schedule = [
            ..GeneratePeriodicEvents(Diablo4EventType.Helltide, new DateTimeOffset(2026, 05, 09, 12, 0, 0, TimeSpan.Zero), 60, TimeSpan.FromMinutes(55)),
            ..GeneratePeriodicEvents(Diablo4EventType.Legion, new DateTimeOffset(2026, 05, 09, 14, 0, 0, TimeSpan.Zero), 25, TimeSpan.FromMinutes(10)),
            ..GeneratePeriodicEvents(Diablo4EventType.WorldBoss, new DateTimeOffset(2026, 05, 09, 14, 30, 0, TimeSpan.Zero), 210, TimeSpan.FromMinutes(2)),
        ];

        schedule = schedule.Where(evt => evt.EndTime >= DateTimeOffset.UtcNow)
           .OrderBy(evt => evt.StartTime)
           .ToList();

        lock (_refreshLock)
        {
            _events.Clear();
            _events.AddRange(schedule);
        }
    }

    private IEnumerable<Diablo4Event> GeneratePeriodicEvents(Diablo4EventType type, DateTimeOffset fixedStart, int minutesBetween, TimeSpan duration)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        int cyclesSinceFixed = ((int)(now - fixedStart).TotalMinutes / minutesBetween) - 1;
        DateTimeOffset start = fixedStart.AddMinutes(cyclesSinceFixed * minutesBetween);

        DateTimeOffset cutoff = now.AddHours(24);

        while (start <= cutoff)
        {
            var evt = new Diablo4Event
            {
                StartTime = start,
                EndTime = start + duration,
                Type = type,
            };

            if (now < evt.EndTime)
            {
                yield return evt;
            }

            start = start.AddMinutes(minutesBetween);
        }
    }

    private bool IsWithinDisplayWindow(Diablo4Event evt)
    {
        return IsWithinDisplayWindow(evt, DateTimeOffset.UtcNow);
    }

    private bool IsWithinDisplayWindow(Diablo4Event evt, DateTimeOffset now)
    {
        if (evt.StartTime >= now)
        {
            return true;
        }

        return evt.EndTime > now;
    }

    private string GetEventCardClass(Diablo4Event evt)
    {
        var now = DateTimeOffset.UtcNow;
        return now >= evt.StartTime && now < evt.EndTime
            ? "d4-event-card d4-event-card-lingering"
            : "d4-event-card";
    }

    private bool ShouldShowNotificationToggle =>
        !string.IsNullOrWhiteSpace(_profile?.PushoverUserKey);

    private bool HasNotification(string eventKey)
    {
        lock (_refreshLock)
        {
            return _notifications.Any(notification => notification.Key == eventKey);
        }
    }

    private async Task ToggleNotificationAsync(Diablo4EventType type, string key, DateTimeOffset occursAt)
    {
        if (ProfileId is null || !ShouldShowNotificationToggle)
        {
            return;
        }

        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();

        List<Diablo4EventNotification> existingNotifications = await dbContext.Diablo4EventNotifications
            .Where(notification => notification.ProfileId == ProfileId.Value
                                   && notification.Key == key)
            .ToListAsync();

        if (existingNotifications.Count > 0)
        {
            dbContext.Diablo4EventNotifications.RemoveRange(existingNotifications);
            await dbContext.SaveChangesAsync();

            lock (_refreshLock)
            {
                _notifications.RemoveAll(notification => notification.Key == key);
            }
        }
        else
        {
            if (occursAt <= DateTimeOffset.UtcNow.Add(_notificationEnableCutoff))
            {
                return;
            }

            var notification = new Diablo4EventNotification
            {
                ProfileId = ProfileId.Value,
                Key = key,
                EventText = type switch
                {
                    Diablo4EventType.Helltide  => "Helltide",
                    Diablo4EventType.Legion    => "Legion",
                    Diablo4EventType.WorldBoss => "World Boss",
                    _                          => throw new ArgumentOutOfRangeException(nameof(type), type, null),
                },
                OccursAt = occursAt,
            };

            dbContext.Diablo4EventNotifications.Add(notification);
            await dbContext.SaveChangesAsync();

            lock (_refreshLock)
            {
                _notifications.Add(notification);
            }
        }

        await _eventBus.PublishAsync(new Diablo4EventNotificationsUpdatedEvent
        {
            ProfileId = ProfileId.Value,
        });

        await InvokeAsync(StateHasChanged);
    }

    private void StartRefreshLoop()
    {
        if (_refreshLoopStarted)
        {
            return;
        }

        _refreshLoopStarted = true;
        _refreshLoopCancellationTokenSource = new CancellationTokenSource();
        _refreshLoopTask = Task.Run(() => RefreshLoopAsync(_refreshLoopCancellationTokenSource.Token));
    }

    private async Task RefreshLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(_refreshCheckInterval);

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            if (_isDisposed)
            {
                return;
            }

            _currentLocalTime = DateTime.Now;

            DateTimeOffset refreshThreshold = DateTimeOffset.UtcNow - _refreshGracePeriod;
            bool shouldRefresh;
            lock (_refreshLock)
            {
                shouldRefresh = _events.Any(evt => evt.EndTime <= refreshThreshold);
            }

            if (shouldRefresh)
            {
                RefreshSchedule();
            }

            await InvokeAsync(StateHasChanged);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        _notificationsUpdatedSubscription?.Dispose();

        if (_refreshLoopCancellationTokenSource is not null)
        {
            await _refreshLoopCancellationTokenSource.CancelAsync();
        }

        if (_refreshLoopTask is not null)
        {
            try
            {
                await _refreshLoopTask;
            }
            catch (OperationCanceledException)
            {
                // Expected during disposal.
            }
        }

        _refreshLoopCancellationTokenSource?.Dispose();

        if (!_countdownStarted)
        {
            return;
        }

        try
        {
            await _jsRuntime.InvokeVoidAsync("gameTools.diablo4EventTimers.stop");
        }
        catch (JSDisconnectedException)
        {
            // Ignore disconnect during disposal.
        }
    }
}