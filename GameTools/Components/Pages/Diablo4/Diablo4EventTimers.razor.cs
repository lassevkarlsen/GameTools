using GameTools.Components.Pages.Diablo4.Models;
using GameTools.Components.Pages.Enshrouded;
using Microsoft.JSInterop;

namespace GameTools.Components.Pages.Diablo4;

public partial class Diablo4EventTimers : IAsyncDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJSRuntime _jsRuntime;
    private readonly Lock _refreshLock = new();

    private readonly List<Diablo4Event> _events = [];
    private readonly TimeSpan _refreshGracePeriod = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _refreshCheckInterval = TimeSpan.FromSeconds(1);

    private bool _countdownStarted;
    private bool _refreshLoopStarted;
    private bool _isDisposed;
    private bool _refreshInProgress;
    private CancellationTokenSource? _refreshLoopCancellationTokenSource;
    private Task? _refreshLoopTask;

    public Diablo4EventTimers(IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        SetPageTitle?.Invoke("Diablo 4 :: Event Timers");

        await RefreshTimersAsync();
        StartRefreshLoop();
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

    private async Task RefreshTimersAsync()
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
            using HttpClient client = _httpClientFactory.CreateClient("Diablo4");
            Diablo4Schedule? schedule = await client.GetFromJsonAsync<Diablo4Schedule>("https://helltides.com/api/schedule");
            List<Diablo4Event> refreshedEvents = [];

            if (schedule is not null)
            {
                refreshedEvents.AddRange(schedule.WorldBosses.Where(evt => evt.StartTime >= DateTimeOffset.Now).OrderBy(ev => ev.Timestamp).Take(2));
                refreshedEvents.AddRange(schedule.LegionEvents.Where(evt => evt.StartTime >= DateTimeOffset.Now).OrderBy(ev => ev.Timestamp).Take(2));
                refreshedEvents.AddRange(schedule.Helltides.Where(evt => evt.StartTime >= DateTimeOffset.Now).OrderBy(ev => ev.Timestamp).Take(2));
                refreshedEvents.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

                DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddHours(12);
                refreshedEvents.RemoveAll(ev => ev.StartTime >= cutoff);
            }

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
                await RefreshTimersAsync();
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