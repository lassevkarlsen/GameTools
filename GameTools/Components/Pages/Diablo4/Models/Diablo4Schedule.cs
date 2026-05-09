using System.Text.Json.Serialization;

namespace GameTools.Components.Pages.Diablo4.Models;

[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
public class Diablo4Schedule
{
    [JsonPropertyName("world_boss")]
    public List<Diablo4WorldBoss> WorldBosses { get; } = [];

    [JsonPropertyName("legion")]
    public List<Diablo4Legion> LegionEvents { get; } = [];

    [JsonPropertyName("helltide")]
    public List<Diablo4Helltide> Helltides { get; } = [];
}