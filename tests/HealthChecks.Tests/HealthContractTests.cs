using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.Tests;

public class HealthContractTests
{
    [Fact]
    public async Task LivenessContinuaSaudavelSemConsultarDependencia()
    {
        var dependency = new DependencyHandler(HttpStatusCode.ServiceUnavailable);
        using var factory = Configure(dependency);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        Assert.Equal(0, dependency.Requests);
    }

    [Theory]
    [InlineData(HttpStatusCode.NoContent, HttpStatusCode.OK, "Healthy")]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable, "Unhealthy")]
    [InlineData(HttpStatusCode.Redirect, HttpStatusCode.ServiceUnavailable, "Unhealthy")]
    public async Task ReadinessRefleteResultadoHttpDaDependencia(
        HttpStatusCode upstreamStatus,
        HttpStatusCode expectedStatus,
        string expectedBody)
    {
        var dependency = new DependencyHandler(upstreamStatus);
        using var factory = Configure(dependency);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health/ready");

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal(expectedBody, await response.Content.ReadAsStringAsync());
        Assert.Equal(1, dependency.Requests);
    }

    private static WebApplicationFactory<Program> Configure(HttpMessageHandler dependency)
    {
        var factory = new WebApplicationFactory<Program>();
        return factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddHttpClient("dependency")
                .ConfigurePrimaryHttpMessageHandler(() => dependency)));
    }

    private sealed class DependencyHandler(HttpStatusCode status) : HttpMessageHandler
    {
        public int Requests { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests++;
            return Task.FromResult(new HttpResponseMessage(status));
        }
    }
}
