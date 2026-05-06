using OpenPdc.Client.Models;

namespace OpenPdc.Client;

public interface IOpenPdcClient
{
    /// <summary>
    /// Fetches a single page of PDC items.
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="limit">Page size (items per page). Pass <c>null</c> to use the API default.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PdcItemsResponse> GetItemsAsync(
        int page = 1,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams every PDC item by transparently following the API's pagination.
    /// </summary>
    /// <param name="pageSize">Page size to request from the upstream API.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    IAsyncEnumerable<PdcItem> GetAllItemsAsync(
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}
