using GameTools.Components.Pages.Diablo4.Models;
using GameTools.Components.Pages.Enshrouded;

namespace GameTools.Components.Pages.Diablo4;

public partial class Diablo4EventTimers
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly List<Diablo4Event> _events = [];

    public Diablo4EventTimers(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        SetPageTitle?.Invoke("Diablo 4 :: Event Timers");

        await RefreshTimersAsync();
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
}