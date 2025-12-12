using Microsoft.AspNetCore.Components;

namespace GameTools.Components.Pages;

public partial class Home
{
    [Inject]
    public NavigationManager? NavigationManager { get; set; }

    protected override Task OnInitializedAsync()
    {
        SetPageTitle?.Invoke("Home");
        return base.OnInitializedAsync();
    }

    private Task CreateSession()
    {
        NavigationManager!.NavigateTo($"/{Guid.NewGuid()}/", forceLoad: true);
        return Task.CompletedTask;
    }
}