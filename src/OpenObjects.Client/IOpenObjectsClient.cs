using OpenObjects.Client.Models;

namespace OpenObjects.Client;

/// <summary>
/// Client for the OpenObjects <c>/api/v2/objects</c> REST API.
/// </summary>
public interface IOpenObjectsClient
{
    /// <summary>
    /// Posts a new object to the OpenObjects API.
    /// </summary>
    /// <param name="request">The object to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created object as returned by the API.</returns>
    Task<ObjectResponse> PostObjectAsync(
        CreateObjectRequest request,
        CancellationToken cancellationToken = default);
}
