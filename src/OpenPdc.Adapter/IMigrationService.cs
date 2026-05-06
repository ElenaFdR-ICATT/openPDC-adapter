namespace OpenPdc.Adapter;

public interface IMigrationService
{
    /// <summary>
    /// Streams all items from OpenPDC and posts each one to OpenObjects.
    /// </summary>
    Task RunAsync(CancellationToken cancellationToken = default);
}
