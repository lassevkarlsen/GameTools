namespace GameTools.Workers.Events;

public class TimersEditedForProfileEvent
{
    public required Guid ProfileId { get; init; }
}