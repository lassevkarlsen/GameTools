using GameTools.Components.Pages.Diablo4.Models;
using GameTools.Components.Pages.Enshrouded;

namespace GameTools.Components.Pages.Diablo4;

public partial class Diablo4EventTimers
{
    private readonly IHttpClientFactory _httpClientFactory;

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
        Diablo4Schedule? model = await client.GetFromJsonAsync<Diablo4Schedule>("https://helltides.com/api/schedule");
    }
}