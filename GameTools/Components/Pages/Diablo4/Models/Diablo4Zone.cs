using System.Diagnostics;
using System.Text.Json.Serialization;

namespace GameTools.Components.Pages.Diablo4.Models;

[DebuggerDisplay("Zone {Name}")]
public class Diablo4Zone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("isWhisper")]
    public bool IsWhisper { get; set; }

    [JsonPropertyName("boss")]
    public string? Boss { get; set; }
}