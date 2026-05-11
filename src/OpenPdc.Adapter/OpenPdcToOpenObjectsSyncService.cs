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
        // Collect all PDC items — we need the full ID set for orphan detection later.
        var pdcItems = new List<PdcItem>();
        await foreach (var item in pdcClient.GetAllItemsAsync(cancellationToken: cancellationToken))
            pdcItems.Add(item);
        logger.LogInformation("Collected {Count} PDC item(s).", pdcItems.Count);

        // insert or update every PDC item into OpenObjects
        var itemCount = 0;
        foreach (var item in pdcItems)
        {
            var dataUrl = $"{options.PdcItemBaseUrl.TrimEnd('/')}/{item.Id}";
            IReadOnlyList<ObjectResponse> matches;
            try
            {
                matches = await objectsClient.FindObjectsByDataUrlAsync(dataUrl, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to look up PDC item {Id} in OpenObjects. Continuing with the next item.", item.Id);
                continue;
            }

            var request = MapToRequest(item);
            try
            {
                if (matches.Count > 0)
                {
                    await objectsClient.PutObjectAsync(matches[0].Uuid, request, cancellationToken);

                    // If there are multiple matches, keep the first and delete the rest to clean up duplicates.
                    // This is not much likely to happen but necessary because the OpenObjects API doesn't enforce uniqueness on the data.url field.
                    for (var i = 1; i < matches.Count; i++)
                    {
                        await objectsClient.DeleteObjectAsync(matches[i].Uuid, cancellationToken);
                        logger.LogWarning("Deleted duplicate object {Uuid} for PDC item {Id}.", matches[i].Uuid, item.Id);
                    }
                }
                else
                {
                    await objectsClient.PostObjectAsync(request, cancellationToken);
                }
                itemCount++;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to process PDC item {Id} ({Done}/{Total} processed before failure). Continuing sync.", item.Id, itemCount, pdcItems.Count);
                continue;
            }
        }
        logger.LogInformation("Processed {Count} item(s).", itemCount);

        // Delete objects in OpenObjects whose PDC item no longer exists.
        var pdcIds = pdcItems.Select(i => i.Id).ToHashSet();
        var toDelete = new List<(Guid Uuid, long ItemId)>();
        try
        {
            await foreach (var obj in objectsClient.GetAllKennisartikelObjectsAsync(options.ObjectTypeUrl, cancellationToken))
            {
                var dataUrl = obj.Record?.Data?.Url;
                if (dataUrl is null) 
                    continue;

                var lastSegment = dataUrl.TrimEnd('/').Split('/')[^1];
                if (long.TryParse(lastSegment, out var itemId) && !pdcIds.Contains(itemId))
                    toDelete.Add((obj.Uuid, itemId));
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to retrieve existing OpenObjects records for orphan check. Aborting sync.");
            return;
        }

        var deletedCount = 0;
        foreach (var (uuid, itemId) in toDelete)
        {
            try
            {
                await objectsClient.DeleteObjectAsync(uuid, cancellationToken);
                deletedCount++;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to delete orphaned object {Uuid} (pdc id {ItemId}), {Deleted}/{Total} deleted before failure. Continuing orphan check.", uuid, itemId, deletedCount, toDelete.Count);
                continue;
            }
        }

        logger.LogInformation("Done. Processed {itemCount} item(s), deleted {DeletedCount} orphan(s).", itemCount, deletedCount);
    }

    private CreateObjectRequest MapToRequest(PdcItem item) =>
        new()
        {
            Type = $"{options.ObjectTypeUrl}",
            Record = new ObjectRecord
            {
                TypeVersion = options.ObjectTypeVersion,
                StartAt     = DateOnly.FromDateTime(DateTime.UtcNow),
                Data = new ObjectData
                {
                    Url             = $"{options.PdcItemBaseUrl}/{item.Id}",
                    Uuid            = Guid.Empty.ToString(),
                    UpnUri          = "unknown",
                    PublicatieDatum = item.DateModified is { } dto ? DateOnly.FromDateTime(dto.UtcDateTime) : null,
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
                            Taal           = item.Language == "en" ? Taal.En : Taal.Nl,
                            Titel          = item.Title,
                            Tekst          = item.Content,
                            DatumWijziging = item.DateModified,
                        }
                    ],
                    BeschikbareTalen = [item.Language ?? "nl"],
                },
            },
        };
}
