namespace GameTools.Workers.Events;

public class Diablo4EventNotificationsUpdatedEvent
{
    public required Guid ProfileId { get; init; }
}