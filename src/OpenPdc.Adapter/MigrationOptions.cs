namespace OpenPdc.Adapter;

public sealed class MigrationOptions
{
    public string ObjectTypeUrl { get; set; } = string.Empty;

    public string PdcItemBaseUrl { get; set; } = string.Empty;

    public string OwmsUrl { get; set; } = string.Empty;

    public string OwmsIdentifier { get; set; } = string.Empty;

    public DateTimeOffset OwmsEndDate { get; set; }

    public string Doelgroep { get; set; } = string.Empty;
}
