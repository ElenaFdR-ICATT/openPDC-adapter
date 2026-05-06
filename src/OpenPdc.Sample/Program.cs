using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenObjects.Client;
using OpenPdc.Adapter;
using OpenPdc.Client;

Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

static string Require(IConfiguration cfg, string key) =>
    cfg[key] is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"'{key}' is required. Set it in .env or as an environment variable.");

var services = new ServiceCollection();

services.AddOpenPdcClient(o =>
{
    o.BaseUrl = Require(config, "OpenPdc:BaseUrl");
});

services.AddOpenObjectsClient(o =>
{
    o.BaseUrl = config["OpenObjects:BaseUrl"] ?? OpenObjectsClientOptions.DefaultBaseUrl;
    o.Token   = Require(config, "OpenObjects:Token");
});

services.AddMigrationService(o =>
{
    o.ObjectTypeUrl  = Require(config, "Migration:ObjectTypeUrl");
    o.PdcItemBaseUrl = Require(config, "Migration:PdcItemBaseUrl");
    o.OwmsUrl        = Require(config, "Migration:OwmsUrl");
    o.OwmsIdentifier = Require(config, "Migration:OwmsIdentifier");
    o.OwmsEndDate    = DateTimeOffset.Parse(config["Migration:OwmsEndDate"] ?? "2099-12-31T23:59:59Z");
    o.Doelgroep      = Require(config, "Migration:Doelgroep");
});

await using var provider = services.BuildServiceProvider();

var migration = provider.GetRequiredService<IMigrationService>();

Console.WriteLine("Starting migration...");
await migration.RunAsync();
