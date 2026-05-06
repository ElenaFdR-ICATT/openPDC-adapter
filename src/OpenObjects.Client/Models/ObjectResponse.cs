using System.Text.Json.Serialization;

namespace OpenObjects.Client.Models;

public sealed class ObjectResponse
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("uuid")]
    public Guid Uuid { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("record")]
    public ObjectRecord? Record { get; init; }
}
