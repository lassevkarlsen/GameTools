using Microsoft.AspNetCore.Components;

namespace GameTools.Components.Pages;

public class BasePage : ComponentBase
{
    [CascadingParameter]
    public Action<string>? SetPageTitle { get; set; }

    [CascadingParameter]
    public Action<Guid>? SetProfileId { get; set; }

    [Parameter]
    public string? ProfileIdString { get; set; }

    public Guid? ProfileId { get; set; }

    public bool IsAuthenticated { get; set; }

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
}