using GameTools.Database;

using Microsoft.AspNetCore.Components;

namespace GameTools.Components.Pages;

public partial class LandingPage
{
    protected override string? LandingPageValue => null;

    [Inject]
    public NavigationManager? NavigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (!IsAuthenticated || ProfileId is null || NavigationManager is null || ProfilePreferences is null)
        {
            NavigationManager?.NavigateTo("/", false);
            return;
        }

        string? value = await ProfilePreferences.GetPreference(ProfileId.Value, LandingPageConstants.PreferenceKey);
        string relativePath = LandingPageConstants.ToRelativePath(value ?? LandingPageConstants.DefaultValue);
        string query = new Uri(NavigationManager.Uri, UriKind.Absolute).Query;
        string path = string.IsNullOrWhiteSpace(relativePath)
            ? $"/{ProfileIdString}{query}"
            : $"/{ProfileIdString}/{relativePath}{query}";

        NavigationManager.NavigateTo(path, false);
    }
}