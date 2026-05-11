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

    /// <summary>
    /// Streams all objects of the given type from the OpenObjects API.
    /// </summary>
    IAsyncEnumerable<ObjectResponse> GetAllKennisartikelObjectsAsync(
        string objectTypeUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all objects whose <c>data.url</c> field matches <paramref name="dataUrl"/> exactly.
    /// Returns an empty list when no match is found. More than one result indicates duplicates.
    /// </summary>
    Task<IReadOnlyList<ObjectResponse>> FindObjectsByDataUrlAsync(string dataUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the object with the given UUID.
    /// </summary>
    Task<ObjectResponse> PutObjectAsync(Guid uuid, CreateObjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the object with the given UUID.
    /// </summary>
    Task DeleteObjectAsync(Guid uuid, CancellationToken cancellationToken = default);
}
