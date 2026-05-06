using OpenObjects.Client;
using OpenObjects.Client.Models;
using OpenPdc.Client;

namespace OpenPdc.Adapter;

public sealed class MigrationService(
    IOpenPdcClient pdcClient,
    IOpenObjectsClient objectsClient,
    MigrationOptions options) : IMigrationService
{
    private readonly IOpenPdcClient _pdcClient = pdcClient;
    private readonly IOpenObjectsClient _objectsClient = objectsClient;
    private readonly MigrationOptions _options = options;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var count = 0;
        await foreach (var item in _pdcClient.GetAllItemsAsync(cancellationToken: cancellationToken))
        {
            var request = new CreateObjectRequest
            {
                Type = _options.ObjectTypeUrl,
                Record = new ObjectRecord
                {
                    TypeVersion = 1,
                    StartAt     = DateOnly.FromDateTime(DateTime.UtcNow),
                    Data = new ObjectData
                    {
                        Url             = $"{_options.PdcItemBaseUrl}/{item.Id}",
                        Uuid            = Guid.NewGuid().ToString(),
                        UpnUri          = "test",
                        PublicatieDatum = DateOnly.FromDateTime(DateTime.UtcNow),
                        ProductAanwezig = true,
                        Doelgroep       = _options.Doelgroep,
                        VerantwoordelijkeOrganisatie = new VerantwoordelijkeOrganisatie
                        {
                            Url            = _options.OwmsUrl,
                            OwmsIdentifier = _options.OwmsIdentifier,
                            OwmsEndDate    = _options.OwmsEndDate,
                        },
                        Vertalingen = [
                            new Vertaling
                            {
                                Taal           = item.Language ?? "nl",
                                Titel          = item.Title,
                                Tekst          = item.Content,
                                DatumWijziging = item.DateModified,
                            }
                        ],
                        BeschikbareTalen = ["nl"],
                    },
                },
            };

            var created = await _objectsClient.PostObjectAsync(request, cancellationToken);
            count++;
            Console.WriteLine($"  [{count}] [{item.Id}] {item.Title} → uuid: {created.Uuid}");
        }

        Console.WriteLine($"\nDone. Posted {count} item(s).");
    }
}
