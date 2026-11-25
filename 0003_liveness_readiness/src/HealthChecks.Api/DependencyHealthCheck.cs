using Microsoft.Extensions.Diagnostics.HealthChecks;

internal sealed class DependencyHealthCheck(IHttpClientFactory clientFactory) : IHealthCheck
{
    public const string ClientName = "dependency";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var client = clientFactory.CreateClient(ClientName);
        using var response = await client.GetAsync(
            client.BaseAddress,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        return response.IsSuccessStatusCode
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy();
    }
}
