using Microsoft.Extensions.DependencyInjection;

namespace OpenPdc.Client;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddOpenPdcClient(
        this IServiceCollection services,
        Action<OpenPdcClientOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new OpenPdcClientOptions();
        configure?.Invoke(options);

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            throw new InvalidOperationException("OpenPdcClientOptions.BaseUrl must be set.");
        }

        // Ensure trailing slash so that relative URIs combine correctly.
        var baseUrl = options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/";

        return services.AddHttpClient<IOpenPdcClient, OpenPdcClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = options.Timeout;
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OpenPdc.Client/1.0");
        });
    }
}
