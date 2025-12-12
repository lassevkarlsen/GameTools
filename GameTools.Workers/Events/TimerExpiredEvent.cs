using GameTools.Database;

namespace GameTools.Workers.Events;

public record TimerExpiredEvent(GameTimer Timer);