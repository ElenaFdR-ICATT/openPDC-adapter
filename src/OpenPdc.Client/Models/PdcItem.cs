using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenPdc.Client.Models;

/// <summary>
/// Represents a single PDC (Producten- en Dienstencatalogus) item as exposed by the
/// <c>owc/pdc/v1/items/</c> WordPress REST endpoint.
/// </summary>
public sealed class PdcItem
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("language")]
    public string? Language { get; init; }

    [JsonPropertyName("date")]
    public DateTimeOffset? Date { get; init; }

    [JsonPropertyName("date_modified")]
    public DateTimeOffset? DateModified { get; init; }
}
