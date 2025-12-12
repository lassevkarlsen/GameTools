using GameTools.Database;
using GameTools.Workers.Events;

using Humanizer;

using LVK;
using LVK.Events;
using LVK.Pushover;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameTools.Workers.TimerNotifications;

public class TimerNotificationsWorker : BackgroundService
{
    private readonly IDbContextFactory<GameToolsDbContext> _dbContextFactory;
    private readonly IEventBus _eventBus;
    private readonly ILogger<TimerNotificationsWorker> _logger;
    private readonly IPushover _pushover;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;

    public TimerNotificationsWorker(IDbContextFactory<GameToolsDbContext> dbContextFactory, IEventBus eventBus,
            ILogger<TimerNotificationsWorker> logger, IPushover pushover,
            IConfiguration configuration)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pushover = pushover ?? throw new ArgumentNullException(nameof(pushover));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _baseUrl = _configuration["App:BaseUrl"] ?? throw new InvalidOperationException("App:BaseUrl not set");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var cts = new CancellationTokenSource();
            await using CancellationTokenRegistration registration = stoppingToken.Register(() =>
            {
                _logger.LogInformation("Stopping timer notification loop");
                cts.Cancel();
            });
            using IDisposable subscription = _eventBus.Subscribe<TimersUpdatedEvent>(_ =>
            {
                _logger.LogInformation("Timers updated, restarting timer notification loop");
                cts.Cancel();
            });

            try
            {
                await NotifyOnTimersAsync(cts.Token);
            }
            catch (TaskCanceledException)
            {
                // Do nothing here
            }
        }
    }

    private async Task NotifyOnTimersAsync(CancellationToken cancellationToken)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var timers = dbContext.GameTimers
           .Where(gt => gt.ElapsesAt != null) // elapsesAt == null --> timer is paused
           .ToList();

        while (timers.Any())
        {
            GameTimer first = timers.OrderBy(gt => gt.ElapsesAt).First();
            TimeSpan timeUntil = first.ElapsesAt!.Value - DateTimeOffset.UtcNow;

            if (timeUntil > TimeSpan.Zero)
            {
                _logger.LogInformation("Timer #{FirstId}: {FirstName} expires in {TimeUntil}", first.Id, first.Name, timeUntil);
                await Task.Delay(timeUntil, cancellationToken);
            }

            await TryNotifyUser(first);
            await _eventBus.PublishAsync(new TimerExpiredEvent(first), CancellationToken.None);
            first.CompletionProcessed = true;
            timers.Remove(first);
            first.ElapsesAt = null;
            await dbContext.SaveChangesAsync(CancellationToken.None);
            await _eventBus.PublishAsync(new TimersEditedForProfileEvent
            {
                ProfileId = first.ProfileId,
            }, CancellationToken.None);

            _logger.LogInformation("Timer #{FirstId}: {FirstName} expired", first.Id, first.Name);
        }

        _logger.LogInformation("No timers to wait for, sleeping until something changes");
        await cancellationToken;
    }

    private async Task TryNotifyUser(GameTimer timer)
    {
        await using GameToolsDbContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        Profile? profile = await dbContext.Profiles.FirstOrDefaultAsync(pf => pf.Id == timer.ProfileId);
        if (profile == null)
        {
            return;
        }

        if (profile.PushoverUserKey == null || timer.CompletionProcessed)
        {
            return;
        }

        try
        {
            await _pushover.SendAsync(new PushoverNotification
            {
                Message = $"GameTools - Timer '{timer.Name}' has expired, after running for {timer.Duration.Humanize(4)}",
                Url = $"{_baseUrl}{timer.ProfileId}/timers",
                UrlTitle = "Open timer page",
                Priority = PushoverNotificationPriority.Normal,
                TargetUser = profile.PushoverUserKey,
                Title = "GameTools timer expired", });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error sending pushover notification");
        }
    }
}