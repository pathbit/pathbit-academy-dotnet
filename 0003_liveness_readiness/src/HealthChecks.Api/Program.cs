using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
const string readinessTag = "ready";

if (!Uri.TryCreate(builder.Configuration["Dependency:HealthUrl"], UriKind.Absolute, out var dependencyUrl)
    || (dependencyUrl.Scheme != Uri.UriSchemeHttp && dependencyUrl.Scheme != Uri.UriSchemeHttps))
{
    throw new InvalidOperationException("Dependency:HealthUrl deve ser uma URL HTTP ou HTTPS absoluta.");
}

builder.Services
    .AddHttpClient(DependencyHealthCheck.ClientName, client =>
    {
        client.BaseAddress = dependencyUrl;
        client.Timeout = TimeSpan.FromSeconds(2);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

builder.Services
    .AddHealthChecks()
    .AddCheck<DependencyHealthCheck>(
        DependencyHealthCheck.ClientName,
        tags: [readinessTag],
        timeout: TimeSpan.FromSeconds(3));

var app = builder.Build();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains(readinessTag)
});

app.Run();

public partial class Program;
