using Microsoft.AspNetCore.Components;

namespace GameTools.Components.Pages;

public partial class Home
{
    [CascadingParameter]
    public Action<string>? SetPageTitle { get; set; }

    protected override Task OnInitializedAsync()
    {
        SetPageTitle?.Invoke("Home");
        return base.OnInitializedAsync();
    }
}