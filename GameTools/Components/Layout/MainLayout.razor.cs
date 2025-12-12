using GameTools.Workers.Events;

using LVK.Events;

using Microsoft.AspNetCore.Components;

using Radzen;

namespace GameTools.Components.Layout;

public partial class MainLayout : IAsyncDisposable
{
    private NavigationManager? _navigationManager;
    private readonly IEventBus _eventBus;
    private readonly NotificationService _notificationService;

    private bool _sidebarExpanded = true;

    private string? _version;
    private string? _branch;

    private string _pageTitle = "Home";
    private Guid? _profileId;

    private IDisposable? _subscription;

    public MainLayout(NavigationManager navigationManager, IEventBus eventBus, NotificationService notificationService)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        string idFileName = "git_id.txt";
        if (File.Exists(idFileName))
        {
            using var reader = new StreamReader(idFileName);
            _version = reader.ReadLine();
            _branch = reader.ReadLine();
        }
        else
        {
            _version = "0000000000000000";
            _branch = "develop";
        }

        _subscription ??= _eventBus.Subscribe<TimerExpiredEvent>(OnTimerExpired);
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
        if (_subscription is IAsyncDisposable subscriptionAsyncDisposable)
        {
            await subscriptionAsyncDisposable.DisposeAsync();
        }
        else if (_subscription != null)
        {
            _subscription.Dispose();
        }
    }
}