using System.Text.Json.Serialization;

namespace OpenPdc.Client.Models;

public sealed class PdcItemsResponse
{
    [JsonPropertyName("data")]
    public IReadOnlyList<PdcItem> Data { get; init; } = [];

    [JsonPropertyName("pagination")]
    public Pagination Pagination { get; init; } = new();
}
