using Microsoft.AspNetCore.Components;

namespace GameTools.Components.Pages;

public class BasePage : ComponentBase
{
    [CascadingParameter]
    public Action<string>? SetPageTitle { get; set; }

    [CascadingParameter]
    public Action<Guid>? SetUserId { get; set; }

    [Parameter]
    public string? UserIdString { get; set; }

    public Guid? UserId { get; set; }

    public bool IsAuthenticated { get; set; }

    protected override Task OnInitializedAsync()
    {
        IsAuthenticated = Guid.TryParse(UserIdString, out Guid userId);
        if (IsAuthenticated)
        {
            UserId = userId;
            SetUserId?.Invoke(UserId.Value);
        }

        return base.OnInitializedAsync();
    }
}