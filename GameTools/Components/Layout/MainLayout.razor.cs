using GameTools.Workers.Events;

using LVK.Events;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

using Radzen;

namespace GameTools.Components.Layout;

public partial class MainLayout : IAsyncDisposable
{
    private readonly NavigationManager _navigationManager;
    private readonly IEventBus _eventBus;
    private readonly NotificationService _notificationService;
    private readonly CookieThemeService _cookieThemeService;
    private readonly IJSRuntime _jsRuntime;
    private readonly ThemeService _themeService;

    private bool _sidebarExpanded = true;

    private string _pageTitle = "Home";
    private Guid? _profileId;

    private bool _hideHeader;

    private string _parameters = string.Empty;

    private IDisposable? _subscription;

    public MainLayout(NavigationManager navigationManager, IEventBus eventBus, NotificationService notificationService,
            CookieThemeService cookieThemeService, IJSRuntime jsRuntime, ThemeService themeService)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _cookieThemeService = cookieThemeService ?? throw new ArgumentNullException(nameof(cookieThemeService));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _subscription ??= _eventBus.Subscribe<TimerExpiredEvent>(OnTimerExpired);
        _navigationManager.LocationChanged += OnLocationChanged;
        UpdateParameters(_navigationManager.Uri);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        UpdateParameters(e.Location);
    }

    private void UpdateParameters(string location)
    {
        var uri = new Uri(location, UriKind.Absolute);
        _parameters = uri.Query;
        _hideHeader = false;

        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("xeneon", out var xeneonValues))
        {
            foreach (var value in xeneonValues)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _hideHeader = true;
                    break;
                }
            }
        }

        _ = _jsRuntime.InvokeVoidAsync("gameTools.setBaseFontSize", _hideHeader);
        if (_hideHeader)
        {
            _themeService.SetTheme("dark");
        }

        StateHasChanged();
    }

    private async Task OnTimerExpired(TimerExpiredEvent arg)
    {
        if (arg.Timer.ProfileId != _profileId)
        {
            return;
        }

        await InvokeAsync(() =>
        {
            _notificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Timer expired",
                Detail = $"Timer '{arg.Timer.Name}' expired",
                Duration = 15_000,
                ShowProgress = true,
                Payload = Guid.NewGuid(), });
        });
    }

    private void SetPageTitle(string title)
    {
        _pageTitle = title;
        StateHasChanged();
    }

    private void SetProfileId(Guid profileId)
    {
        _profileId = profileId;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        _navigationManager.LocationChanged -= OnLocationChanged;

        if (_subscription is IAsyncDisposable subscriptionAsyncDisposable)
        {
            await subscriptionAsyncDisposable.DisposeAsync();
        }
        else if (_subscription != null)
        {
            _subscription.Dispose();
        }
    }

    private Task NavigateToProfilePage()
    {
        _navigationManager.NavigateTo($"/{_profileId}/profile{_parameters}");
        return Task.CompletedTask;
    }
}