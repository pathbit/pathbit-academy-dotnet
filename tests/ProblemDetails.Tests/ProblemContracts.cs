using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ProblemDetails.Tests;

public sealed class ProblemContracts
{
    [Fact]
    public async Task ValidationReturnsStableTypeAndFields()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/reservations/preview", new { sku = "", quantity = 0 });

        var problem = await ReadProblem(response, HttpStatusCode.BadRequest);
        Assert.Equal("urn:pathbit:problem:invalid-reservation", problem.GetProperty("type").GetString());
        Assert.NotEmpty(problem.GetProperty("errors").GetProperty("sku").EnumerateArray());
        Assert.NotEmpty(problem.GetProperty("errors").GetProperty("quantity").EnumerateArray());
    }

    [Theory]
    [InlineData("mouse", 1, HttpStatusCode.NotFound, "urn:pathbit:problem:product-not-found")]
    [InlineData("keyboard", 3, HttpStatusCode.Conflict, "urn:pathbit:problem:insufficient-stock")]
    public async Task DomainProblemsHaveDistinctTypes(
        string sku, int quantity, HttpStatusCode status, string type)
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/reservations/preview", new { sku, quantity });

        var problem = await ReadProblem(response, status);
        Assert.Equal(type, problem.GetProperty("type").GetString());
        if (status == HttpStatusCode.Conflict)
        {
            Assert.Equal(2, problem.GetProperty("availableQuantity").GetInt32());
        }
    }

    [Fact]
    public async Task SuccessUsesARegularJsonDocument()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/reservations/preview", new { sku = "keyboard", quantity = 2 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var reservation = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, reservation.GetProperty("quantity").GetInt32());
    }

    [Fact]
    public async Task MissingRouteAndMalformedJsonHaveProblemBodies()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var configured = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
        using var client = configured.CreateClient();
        using var missing = await client.GetAsync("/unknown-route");
        using var invalidJson = new StringContent("{", Encoding.UTF8, "application/json");
        using var malformed = await client.PostAsync("/reservations/preview", invalidJson);

        await ReadProblem(missing, HttpStatusCode.NotFound);
        await ReadProblem(malformed, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UnexpectedFailureDoesNotExposeTheException()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var configured = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
        using var client = configured.CreateClient();
        using var response = await client.GetAsync("/operations/fail");

        var problem = await ReadProblem(response, HttpStatusCode.InternalServerError);
        Assert.DoesNotContain("simulated stock backend", problem.GetRawText(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("InvalidOperationException", problem.GetRawText(), StringComparison.Ordinal);
    }

    private static async Task<JsonElement> ReadProblem(HttpResponseMessage response, HttpStatusCode status)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal((int)status, body.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("traceId").GetString()));
        return body;
    }
}
