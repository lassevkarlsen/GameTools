using GameTools.Components.Pages.Enshrouded;

namespace GameTools.Components.Pages.Diablo4;

public partial class Diablo4EventTimers
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        SetPageTitle?.Invoke("Diablo 4 :: Event Timers");
    }
}