namespace OpenPdc.Client;

/// <summary>
/// Configuration for <see cref="OpenPdcClient"/>.
/// </summary>
public sealed class OpenPdcClientOptions
{
    /// <summary>
    /// Base URL of the OpenPDC API. Must end with a trailing slash.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Request timeout. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
