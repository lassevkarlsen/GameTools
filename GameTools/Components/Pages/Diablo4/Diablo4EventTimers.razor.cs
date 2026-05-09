using GameTools.Components.Pages.Diablo4.Models;
using GameTools.Components.Pages.Enshrouded;
using Microsoft.JSInterop;

namespace GameTools.Components.Pages.Diablo4;

public partial class Diablo4EventTimers : IAsyncDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJSRuntime _jsRuntime;

    private readonly List<Diablo4Event> _events = [];
    private bool _countdownStarted;

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
        using HttpClient client = _httpClientFactory.CreateClient("Diablo4");
        Diablo4Schedule? schedule = await client.GetFromJsonAsync<Diablo4Schedule>("https://helltides.com/api/schedule");
        _events.Clear();
        if (schedule is not null)
        {
            _events.AddRange(schedule.WorldBosses.OrderBy(ev => ev.Timestamp).Take(2));
            _events.AddRange(schedule.LegionEvents.OrderBy(ev => ev.Timestamp).Take(2));
            _events.AddRange(schedule.Helltides.OrderBy(ev => ev.Timestamp).Take(2));
            _events.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
            DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddHours(12);
            _events.RemoveAll(ev => ev.StartTime >= cutoff);
        }

        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
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