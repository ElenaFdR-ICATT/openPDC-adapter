namespace OpenPdc.Adapter;

public sealed class OpenPdcToOpenObjectsSyncOptions
{
    public string ObjectTypeUrl { get; set; } = string.Empty;

    public int ObjectTypeVersion { get; set; } = 1;

    public string OwmsUrl { get; set; } = string.Empty;

    public string OwmsIdentifier { get; set; } = string.Empty;

    public DateTimeOffset OwmsEndDate { get; set; }

    public string[] WordPressContentTypes { get; set; } = ["product", "pages", "publication"];
}
