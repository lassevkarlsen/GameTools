using System.Diagnostics;
using System.Text.Json.Serialization;

namespace GameTools.Components.Pages.Diablo4.Models;

[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
[DebuggerDisplay("World boss {Name} at {StartTime}")]
public class Diablo4WorldBoss : Diablo4Event
{
    [JsonPropertyName("boss")]
    public string Name { get; set; } = "";

    [JsonPropertyName("zone")]
    public List<Diablo4Zone> Zones { get; } = [];
}