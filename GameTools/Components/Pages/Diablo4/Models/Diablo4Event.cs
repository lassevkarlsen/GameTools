using System.Diagnostics;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Components;

namespace GameTools.Components.Pages.Diablo4.Models;

[DebuggerDisplay("{Type} at {StartTime}")]
public class Diablo4Event
{
    public string Key => $"{Type}:{StartTime.ToUnixTimeSeconds()}";

    public required Diablo4EventType Type { get; init; }

    public required DateTimeOffset StartTime { get; init; }

    public required DateTimeOffset EndTime { get; init; }

    public string DisplayTitle
        => Type switch
        {
            Diablo4EventType.Helltide  => "Helltide",
            Diablo4EventType.Legion    => "Legion",
            Diablo4EventType.WorldBoss => "World Boss",
            _                          => throw new ArgumentOutOfRangeException(nameof(Type)),
        };
}