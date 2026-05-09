using Microsoft.AspNetCore.Components;

using GameTools.Database;

namespace GameTools.Components.Pages;

public class BasePage : ComponentBase
{
    [Inject]
    public IProfilePreferences? ProfilePreferences { get; set; }

    [CascadingParameter]
    public Action<string>? SetPageTitle { get; set; }

    [CascadingParameter]
    public Action<Guid>? SetProfileId { get; set; }

    [Parameter]
    public string? ProfileIdString { get; set; }

    public Guid? ProfileId { get; set; }

    public bool IsAuthenticated { get; set; }

    protected virtual string? LandingPageValue => null;

    protected override Task OnInitializedAsync()
    {
        IsAuthenticated = Guid.TryParse(ProfileIdString, out Guid profileId);
        if (IsAuthenticated)
        {
            ProfileId = profileId;
            SetProfileId?.Invoke(ProfileId.Value);
        }

        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender || !IsAuthenticated || ProfileId is null || string.IsNullOrWhiteSpace(LandingPageValue) || ProfilePreferences == null)
        {
            return;
        }

        await ProfilePreferences.SetPreference(ProfileId.Value, LandingPageConstants.PreferenceKey, LandingPageValue);
    }
}