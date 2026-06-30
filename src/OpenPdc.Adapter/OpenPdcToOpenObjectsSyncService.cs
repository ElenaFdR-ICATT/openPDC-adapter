using Microsoft.Extensions.Logging;
using OpenObjects.Client;
using OpenObjects.Client.Models;
using OpenPdc.Client;
using OpenPdc.Client.Models;

namespace OpenPdc.Adapter;

public sealed class OpenPdcToOpenObjectsSyncService(
    IOpenPdcClient pdcClient,
    IOpenObjectsClient objectsClient,
    OpenPdcToOpenObjectsSyncOptions options,
    ILogger<OpenPdcToOpenObjectsSyncService> logger) : IOpenPdcToOpenObjectsSyncService
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var pdcItems = new List<(PdcItem Item, string ContentType)>();
        foreach (var contentType in options.WordPressContentTypes)
        {
            var before = pdcItems.Count;
            await foreach (var item in pdcClient.GetAllItemsAsync(contentType, cancellationToken: cancellationToken))
                pdcItems.Add((item, contentType));
            var collected = pdcItems.Count - before;
            logger.LogInformation("Collected {Count} PDC item(s) from '{ContentType}'.", collected, contentType);
        }
        logger.LogInformation("Collected {Count} PDC item(s) in total.", pdcItems.Count);

        // Get all existing OpenObjects records once and build a lookup by PDC itemId.
        var existingByItemId = new Dictionary<long, ObjectResponse>();
        var duplicates = new List<(Guid Uuid, long ItemId)>();
        try
        {
            var grouped = new Dictionary<long, List<ObjectResponse>>();
            await foreach (var obj in objectsClient.GetAllObjectsByObjectTypeUrlAsync(options.ObjectTypeUrl, cancellationToken))
            {
                var dataUrl = obj.Record?.Data?.Url;
                if (dataUrl is null) 
                    continue;
                
                var lastSegment = dataUrl.TrimEnd('/').Split('/')[^1];
                if (!long.TryParse(lastSegment, out var itemId)) 
                    continue;

                if (!grouped.TryGetValue(itemId, out var list))
                    grouped[itemId] = list = [];
                list.Add(obj);
            }

            foreach (var (itemId, objects) in grouped)
            {
                existingByItemId[itemId] = objects[0];
                for (var i = 1; i < objects.Count; i++)
                    duplicates.Add((objects[i].Uuid, itemId));
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to retrieve existing OpenObjects records. Aborting sync.");
            return;
        }

        // Remove any duplicates in OpenObjects before syncing.
        foreach (var (uuid, itemId) in duplicates)
        {
            try
            {
                await objectsClient.DeleteObjectAsync(uuid, cancellationToken);
                logger.LogWarning("Deleted duplicate object {Uuid} for PDC item {ItemId}.", uuid, itemId);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to delete duplicate object {Uuid} for PDC item {ItemId}.", uuid, itemId);
            }
        }

        // Insert or update every PDC item.
        var itemCount = 0;
        foreach (var (item, contentType) in pdcItems)
        {
            var request = MapToRequest(item, contentType);
            try
            {
                if (existingByItemId.TryGetValue(item.Id, out var existing))
                    await objectsClient.DeleteObjectAsync(existing.Uuid, cancellationToken);
                await objectsClient.PostObjectAsync(request, cancellationToken);
                itemCount++;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to process PDC item {Id} ({Done}/{Total} processed before failure). Continuing sync.", item.Id, itemCount, pdcItems.Count);
            }
        }
        logger.LogInformation("Processed {Count} item(s).", itemCount);

        // Delete obecjts in OpenObjects whose PDC item no longer exists.
        var pdcIds = pdcItems.Select(i => i.Item.Id).ToHashSet();
        var orphans = existingByItemId.Where(kvp => !pdcIds.Contains(kvp.Key)).ToList();
        var deletedCount = 0;
        foreach (var (itemId, obj) in orphans)
        {
            try
            {
                await objectsClient.DeleteObjectAsync(obj.Uuid, cancellationToken);
                deletedCount++;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to delete orphaned object {Uuid} (pdc id {ItemId}), {Deleted}/{Total} deleted before failure. Continuing orphan check.", obj.Uuid, itemId, deletedCount, orphans.Count);
            }
        }

        logger.LogInformation("Done. Processed {itemCount} item(s), deleted {DeletedCount} orphan(s).", itemCount, deletedCount);
    }

    private CreateObjectRequest MapToRequest(PdcItem item, string contentType) =>
        new()
        {
            Type = $"{options.ObjectTypeUrl}",
            Record = new ObjectRecord
            {
                TypeVersion = options.ObjectTypeVersion,
                StartAt     = DateOnly.FromDateTime(DateTime.UtcNow),
                Data = new ObjectData
                {
                    Url             = $"{pdcClient.BaseUrl}/{contentType}/{item.Id}",
                    // made up per-item UUID — Elasticsearch deduplicates by UUID, so each item needs a unique one
                    Uuid            = $"00000000-0000-0000-0000-{item.Id:D12}",
                    UpnUri          = "unknown",
                    PublicatieDatum = item.Modified is { } dto ? DateOnly.FromDateTime(dto.UtcDateTime) : null,
                    ProductAanwezig = true,
                    Doelgroep       = "eu-burger",
                    VerantwoordelijkeOrganisatie = new VerantwoordelijkeOrganisatie
                    {
                        Url            = options.OwmsUrl,
                        OwmsIdentifier = options.OwmsIdentifier,
                        OwmsEndDate    = options.OwmsEndDate,
                    },
                    Vertalingen = [
                        new Vertaling
                        {
                            Taal           = Taal.Nl,
                            Titel          = item.Title?.Rendered,
                            Tekst          = item.Content?.Rendered,
                            DatumWijziging = item.Modified,
                            DeskMemo       = item.InternalMemo ?? string.Empty,
                        }
                    ],
                    BeschikbareTalen = ["nl"],
                },
            },
        };
}
