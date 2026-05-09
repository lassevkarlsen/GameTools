using GameTools.Database;
using GameTools.Workers.Events;

using LVK.Events;
using LVK.Pushover;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameTools.Workers.Diablo4;

public class Diablo4EventNotificationsWorker : BackgroundService
{
    private static readonly TimeSpan NotificationLeadTime = TimeSpan.FromMinutes(5);

    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;
    private readonly IEventBus _eventBus;
    private readonly ILogger<Diablo4EventNotificationsWorker> _logger;
    private readonly IPushover _pushover;
    private readonly string _baseUrl;

    public Diablo4EventNotificationsWorker(IDbContextFactory<GameToolsDbContext> dbContextFactory,
            IEventBus eventBus, ILogger<Diablo4EventNotificationsWorker> logger, IPushover pushover,
            IConfiguration configuration)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pushover = pushover ?? throw new ArgumentNullException(nameof(pushover));

        _baseUrl = configuration["App:BaseUrl"] ?? throw new InvalidOperationException("App:BaseUrl not set");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var cts = new CancellationTokenSource();
            await using CancellationTokenRegistration registration = stoppingToken.Register(() =>
            {
                _logger.LogInformation("Stopping diablo 4 event notification loop");
                cts.Cancel();
            });
            using IDisposable subscription = _eventBus.Subscribe<Diablo4EventNotificationsUpdatedEvent>(_ =>
            {
                _logger.LogInformation("Diablo 4 notifications updated, restarting notification loop");
                cts.Cancel();
            });

            try
            {
                await NotifyOnDiablo4EventsAsync(cts.Token);
            }
            catch (TaskCanceledException)
            {
                // Do nothing here
            }
        }
    }

    private async Task NotifyOnDiablo4EventsAsync(CancellationToken cancellationToken)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var notifications = await dbContext.Diablo4EventNotifications
            .Where(notification => !notification.NotificationSent)
            .OrderBy(notification => notification.OccursAt)
            .ToListAsync(cancellationToken);

        while (notifications.Any())
        {
            Diablo4EventNotification first = notifications.First();
            DateTimeOffset notifyAt = first.OccursAt - NotificationLeadTime;
            TimeSpan timeUntil = notifyAt - DateTimeOffset.UtcNow;

            if (timeUntil > TimeSpan.Zero)
            {
                _logger.LogInformation("Diablo 4 notification #{NotificationId} for profile {ProfileId} in {TimeUntil}", first.Id,
                    first.ProfileId, timeUntil);
                await Task.Delay(timeUntil, cancellationToken);
            }

            await TryNotifyUser(first);

            first.NotificationSent = true;
            notifications.Remove(first);
            await dbContext.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation("Diablo 4 notification #{NotificationId} marked as sent", first.Id);
        }

        _logger.LogInformation("No Diablo 4 event notifications to wait for, sleeping until something changes");
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    private async Task TryNotifyUser(Diablo4EventNotification notification)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        Profile? profile = await dbContext.Profiles.FirstOrDefaultAsync(pf => pf.Id == notification.ProfileId);
        if (profile == null || string.IsNullOrWhiteSpace(profile.PushoverUserKey))
        {
            return;
        }

        try
        {
            await _pushover.SendAsync(new PushoverNotification
            {
                Message = $"GameTools - Diablo 4 event '{notification.EventText}' starts in 5 minutes",
                Url = $"{_baseUrl}{notification.ProfileId}/diablo4/eventtimers",
                UrlTitle = "Open Diablo 4 event timers",
                Priority = PushoverNotificationPriority.Normal,
                TargetUser = profile.PushoverUserKey,
                Title = $"GameTools Diablo 4 {notification.EventText}",
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error sending pushover notification for Diablo 4 event notification #{NotificationId}",
                notification.Id);
        }
    }
}