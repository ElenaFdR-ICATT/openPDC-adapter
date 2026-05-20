using System.Text.Json.Serialization;

namespace OpenPdc.Client.Models;

public sealed class Pagination
{
    [JsonPropertyName("total_count")]
    public int TotalCount { get; init; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; init; }

    [JsonPropertyName("current_page")]
    public int CurrentPage { get; init; }

    [JsonPropertyName("limit")]
    public int Limit { get; init; }
}
