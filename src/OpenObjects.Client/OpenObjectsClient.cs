using System.Net.Http.Json;
using System.Text.Json;
using OpenObjects.Client.Models;

namespace OpenObjects.Client;

public sealed class OpenObjectsClient : IOpenObjectsClient
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly HttpClient _httpClient;

    public OpenObjectsClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    public async Task<ObjectResponse> PostObjectAsync(
        CreateObjectRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var body = JsonSerializer.Serialize(request, JsonOptions);
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/v2/objects")
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
        };

        var response = await _httpClient
            .SendAsync(message, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new HttpRequestException(
                $"POST {response.RequestMessage?.RequestUri} failed with {(int)response.StatusCode} ({response.ReasonPhrase}).\nResponse body:\n{errorBody}");
        }

        var result = await response.Content
            .ReadFromJsonAsync<ObjectResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InvalidOperationException("OpenObjects API returned an empty response.");
    }
}
