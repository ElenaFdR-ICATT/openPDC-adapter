using System.Text.Json.Serialization;

namespace OpenObjects.Client.Models;

public sealed class ObjectRecord
{
    [JsonPropertyName("typeVersion")]
    public int TypeVersion { get; init; }

    [JsonPropertyName("startAt")]
    public DateOnly StartAt { get; init; }

    [JsonPropertyName("data")]
    public required ObjectData Data { get; init; }
}
