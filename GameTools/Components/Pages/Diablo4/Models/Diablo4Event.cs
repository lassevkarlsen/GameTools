using System.Diagnostics;
using System.Text.Json.Serialization;

namespace GameTools.Components.Pages.Diablo4.Models;

[DebuggerDisplay("{Type} at {StartTime}")]
public class Diablo4Event
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("startTime")]
    public DateTimeOffset StartTime { get; set; }
}