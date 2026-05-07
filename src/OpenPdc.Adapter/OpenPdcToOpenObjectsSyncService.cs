using Microsoft.Extensions.Logging;
using OpenObjects.Client;
using OpenObjects.Client.Models;
using OpenPdc.Client;
using OpenPdc.Client.Models;

namespace OpenPdc.Adapter;

public sealed class OpenPdcToOpenObjectsSyncService(
    IOpenPdcClient pdcClient,
    IOpenObjectsClient objectsClient,
    MigrationOptions options,
    ILogger<OpenPdcToOpenObjectsSyncService> logger) : IMigrationService
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var requests = new List<CreateObjectRequest>();
        await foreach (var item in pdcClient.GetAllItemsAsync(cancellationToken: cancellationToken))
        {
            requests.Add(MapToRequest(item));
        }
        logger.LogInformation("Collected {Count} PDC item(s).", requests.Count);

        var toDelete = new List<Guid>();
        await foreach (var existing in objectsClient.GetAllKennisartikelObjectsAsync(options.ObjectTypeUrl, cancellationToken))
            toDelete.Add(existing.Uuid);

        foreach (var uuid in toDelete)
            await objectsClient.DeleteObjectAsync(uuid, cancellationToken);

        logger.LogInformation("Deleted {Count} existing object(s).", toDelete.Count);

        var postCount = 0;
        foreach (var request in requests)
        {
            var created = await objectsClient.PostObjectAsync(request, cancellationToken);
            postCount++;
            logger.LogDebug("[{PostCount}/{Total}] posted, uuid: {Uuid}", postCount, requests.Count, created.Uuid);
        }

        logger.LogInformation("Done. Synced {Count} item(s).", postCount);
    }

    private CreateObjectRequest MapToRequest(PdcItem item) =>
        new()
        {
            Type = options.ObjectTypeUrl,
            Record = new ObjectRecord
            {
                TypeVersion = 1,
                StartAt     = DateOnly.FromDateTime(DateTime.UtcNow),
                Data = new ObjectData
                {
                    Url             = $"{options.PdcItemBaseUrl}/{item.Id}",
                    Uuid            = Guid.Empty.ToString(),
                    UpnUri          = "unknown",
                    PublicatieDatum = item.DateModified is { } dto ? DateOnly.FromDateTime(dto.UtcDateTime) : null,
                    ProductAanwezig = true,
                    Doelgroep       = options.Doelgroep,
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
