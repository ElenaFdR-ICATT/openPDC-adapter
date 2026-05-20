using System.Text.Json.Serialization;

namespace OpenObjects.Client.Models;

public sealed class ObjectsPage
{
    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("next")]
    public string? Next { get; init; }

    [JsonPropertyName("results")]
    public IReadOnlyList<ObjectResponse> Results { get; init; } = [];
}
