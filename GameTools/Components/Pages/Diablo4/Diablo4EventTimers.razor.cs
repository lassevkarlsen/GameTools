using GameTools.Components.Pages.Diablo4.Models;
using GameTools.Components.Pages.Enshrouded;
using GameTools.Database;
using GameTools.Workers.Events;

using LVK.Events;

using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace GameTools.Components.Pages.Diablo4;

public partial class Diablo4EventTimers : IAsyncDisposable
{
    private const string ShowLegionPreferenceKey = "d4-eventtimers-show-legion";
    private const string ShowHelltidesPreferenceKey = "d4-eventtimers-show-helltides";
    private const string ShowWorldBossesPreferenceKey = "d4-eventtimers-show-worldbosses";

    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;
    private readonly IEventBus _eventBus;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJSRuntime _jsRuntime;
    private readonly IProfilePreferences _profilePreferences;
    private readonly Lock _refreshLock = new();

    private readonly List<Diablo4Event> _events = [];
    private readonly List<Diablo4Event> _fullSchedule = [];
    private readonly List<Diablo4EventNotification> _notifications = [];
    private Profile? _profile;
    private readonly TimeSpan _refreshGracePeriod = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _refreshCheckInterval = TimeSpan.FromSeconds(1);

    private bool _countdownStarted;
    private bool _refreshLoopStarted;
    private bool _isDisposed;
    private bool _refreshInProgress;
    private bool _showLegionEvents = true;
    private bool _showHelltides = true;
    private bool _showWorldBosses = true;
    private CancellationTokenSource? _refreshLoopCancellationTokenSource;
    private Task? _refreshLoopTask;

    public Diablo4EventTimers(IDbContextFactory<GameToolsDbContext> dbContextFactory, IEventBus eventBus,
            IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime, IProfilePreferences profilePreferences)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _profilePreferences = profilePreferences ?? throw new ArgumentNullException(nameof(profilePreferences));
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await LoadEventTypePreferencesAsync();
        await RefreshScheduleAsync();
        await FilterEventsAsync();
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

        _showLegionEvents = await LoadEventTypePreferenceAsync(ShowLegionPreferenceKey);
        _showHelltides = await LoadEventTypePreferenceAsync(ShowHelltidesPreferenceKey);
        _showWorldBosses = await LoadEventTypePreferenceAsync(ShowWorldBossesPreferenceKey);
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
        await PersistEventTypePreferenceAsync(ShowLegionPreferenceKey, value);
        await FilterEventsAsync();
    }

    private async Task OnShowHelltidesChanged(bool value)
    {
        _showHelltides = value;
        await PersistEventTypePreferenceAsync(ShowHelltidesPreferenceKey, value);
        await FilterEventsAsync();
    }

    private async Task OnShowWorldBossesChanged(bool value)
    {
        _showWorldBosses = value;
        await PersistEventTypePreferenceAsync(ShowWorldBossesPreferenceKey, value);
        await FilterEventsAsync();
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

    private async Task FilterEventsAsync()
    {
        lock (_refreshLock)
        {
            if (_refreshInProgress || _isDisposed)
            {
                return;
            }

            _refreshInProgress = true;
        }

        try
        {
            List<Diablo4Event> refreshedEvents = GetFilteredEvents();

            lock (_refreshLock)
            {
                _events.Clear();
                _events.AddRange(refreshedEvents);
            }

            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            lock (_refreshLock)
            {
                _refreshInProgress = false;
            }
        }
    }

    private async Task RefreshScheduleAsync()
    {
        using HttpClient client = _httpClientFactory.CreateClient("Diablo4");
        Diablo4Schedule? schedule = await client.GetFromJsonAsync<Diablo4Schedule>("https://helltides.com/api/schedule");

        List<Diablo4Event> refreshedSchedule = [];
        if (schedule is not null)
        {
            refreshedSchedule.AddRange(schedule.WorldBosses.Where(evt => evt.StartTime >= DateTimeOffset.Now));
            refreshedSchedule.AddRange(schedule.LegionEvents.Where(evt => evt.StartTime >= DateTimeOffset.Now));
            refreshedSchedule.AddRange(schedule.Helltides.Where(evt => evt.StartTime >= DateTimeOffset.Now));
            refreshedSchedule.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        }

        lock (_refreshLock)
        {
            _fullSchedule.Clear();
            _fullSchedule.AddRange(refreshedSchedule);
        }
    }

    private List<Diablo4Event> GetFilteredEvents()
    {
        int enabledEventTypeCount = GetEnabledEventTypeCount();

        List<Diablo4Event> filteredEvents;
        lock (_refreshLock)
        {
            filteredEvents = [];
            filteredEvents.AddRange(_fullSchedule.OfType<Diablo4WorldBoss>()
                .OrderBy(ev => ev.Timestamp)
                .Take(GetPerTypeTakeCount(_showWorldBosses, enabledEventTypeCount)));
            filteredEvents.AddRange(_fullSchedule.OfType<Diablo4Legion>()
                .OrderBy(ev => ev.Timestamp)
                .Take(GetPerTypeTakeCount(_showLegionEvents, enabledEventTypeCount)));
            filteredEvents.AddRange(_fullSchedule.OfType<Diablo4Helltide>()
                .OrderBy(ev => ev.Timestamp)
                .Take(GetPerTypeTakeCount(_showHelltides, enabledEventTypeCount)));
        }

        filteredEvents.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddHours(12);
        filteredEvents.RemoveAll(ev => ev.StartTime >= cutoff);

        return filteredEvents;
    }

    private int GetEnabledEventTypeCount()
    {
        int enabledEventTypeCount = 0;

        if (_showLegionEvents)
        {
            enabledEventTypeCount++;
        }

        if (_showHelltides)
        {
            enabledEventTypeCount++;
        }

        if (_showWorldBosses)
        {
            enabledEventTypeCount++;
        }

        return enabledEventTypeCount;
    }

    private static int GetPerTypeTakeCount(bool showEventType, int enabledEventTypeCount)
    {
        if (!showEventType)
        {
            return 0;
        }

        return enabledEventTypeCount switch
        {
            1 => 6,
            2 => 3,
            _ => 2,
        };
    }

    private bool ShouldShowNotificationToggle =>
        !string.IsNullOrWhiteSpace(_profile?.PushoverUserKey);

    private bool HasNotification(string eventType, string eventId, DateTimeOffset occursAt)
    {
        lock (_refreshLock)
        {
            return _notifications.Any(notification =>
                notification.Type == eventType &&
                notification.EventId == eventId &&
                notification.OccursAt == occursAt);
        }
    }

    private async Task ToggleNotificationAsync(string eventType, string eventId, string eventText, DateTimeOffset occursAt)
    {
        if (ProfileId is null || !ShouldShowNotificationToggle)
        {
            return;
        }

        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();

        List<Diablo4EventNotification> existingNotifications = await dbContext.Diablo4EventNotifications
            .Where(notification => notification.ProfileId == ProfileId.Value
                                   && notification.Type == eventType
                                   && notification.EventId == eventId
                                   && notification.OccursAt == occursAt)
            .ToListAsync();

        if (existingNotifications.Count > 0)
        {
            dbContext.Diablo4EventNotifications.RemoveRange(existingNotifications);
            await dbContext.SaveChangesAsync();

            lock (_refreshLock)
            {
                _notifications.RemoveAll(notification =>
                    notification.Type == eventType &&
                    notification.EventId == eventId &&
                    notification.OccursAt == occursAt);
            }
        }
        else
        {
            var notification = new Diablo4EventNotification
            {
                ProfileId = ProfileId.Value,
                Type = eventType,
                EventId = eventId,
                EventText = eventText,
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

            DateTimeOffset refreshThreshold = DateTimeOffset.UtcNow - _refreshGracePeriod;
            bool shouldRefresh;
            lock (_refreshLock)
            {
                shouldRefresh = _events.Any(evt => evt.StartTime <= refreshThreshold);
            }

            if (shouldRefresh)
            {
                await RefreshScheduleAsync();
                await FilterEventsAsync();
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;

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