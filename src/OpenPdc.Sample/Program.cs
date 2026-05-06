using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenObjects.Client;
using OpenPdc.Adapter;
using OpenPdc.Client;

Env.TraversePath().Load();

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services.AddOpenPdcClient(o =>
{
    o.BaseUrl = config["OpenPdc:BaseUrl"]
                ?? throw new InvalidOperationException("OpenPdc:BaseUrl is required (appsettings.json or .env).");
});

services.AddOpenObjectsClient(o =>
{
    o.BaseUrl = config["OpenObjects:BaseUrl"] ?? OpenObjectsClientOptions.DefaultBaseUrl;
    o.Token   = config["OpenObjects:Token"]
                ?? throw new InvalidOperationException("OpenObjects:Token is required (appsettings.json or .env).");
});

services.AddMigrationService(o =>
{
    o.ObjectTypeUrl  = config["Migration:ObjectTypeUrl"]  ?? string.Empty;
    o.PdcItemBaseUrl = config["Migration:PdcItemBaseUrl"] ?? string.Empty;
    o.OwmsUrl        = config["Migration:OwmsUrl"]        ?? string.Empty;
    o.OwmsIdentifier = config["Migration:OwmsIdentifier"] ?? string.Empty;
    o.OwmsEndDate    = DateTimeOffset.Parse(config["Migration:OwmsEndDate"] ?? "2099-12-31T23:59:59Z");
    o.Doelgroep      = config["Migration:Doelgroep"]      ?? string.Empty;
});

await using var provider = services.BuildServiceProvider();

var migration = provider.GetRequiredService<IMigrationService>();

Console.WriteLine("Starting migration...");
await migration.RunAsync();
