using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogCache.Tests;

public sealed class CacheContracts
{
    [Fact]
    public async Task RepeatedReadsReuseEachQueryVariant()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var brief = await Read(client, "/catalog?view=brief");
        var full = await Read(client, "/catalog?view=full");

        Assert.NotEqual(Generation(brief), Generation(full));
        Assert.Equal(JsonValueKind.Null, brief.GetProperty("description").ValueKind);
        Assert.Equal("Compact USB keyboard", full.GetProperty("description").GetString());
        Assert.Equal(Generation(brief), Generation(await Read(client, "/catalog?view=brief")));
        Assert.Equal(Generation(full), Generation(await Read(client, "/catalog?view=full")));
    }

    [Fact]
    public async Task ChangingPriceRefreshesCatalogAndLeavesIndependentCacheIntact()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        string[] catalogPaths = ["/catalog?view=brief", "/catalog?view=full", "/catalog/summary"];
        var before = new List<long>();
        foreach (var path in catalogPaths)
        {
            before.Add(Generation(await Read(client, path)));
        }
        var about = Generation(await Read(client, "/about"));

        using var updated = await client.PostAsJsonAsync("/catalog/price", new { priceInCents = 12_900 });

        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        for (var index = 0; index < catalogPaths.Length; index++)
        {
            var updatedView = await Read(client, catalogPaths[index]);
            Assert.Equal(12_900, updatedView.GetProperty("priceInCents").GetInt32());
            Assert.Equal(2, updatedView.GetProperty("revision").GetInt32());
            Assert.NotEqual(before[index], Generation(updatedView));
        }
        Assert.Equal(about, Generation(await Read(client, "/about")));
    }

    [Fact]
    public async Task CatalogTagClearsTheGroupWithoutChangingItsRevision()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var catalog = Generation(await Read(client, "/catalog?view=full"));
        var summary = Generation(await Read(client, "/catalog/summary"));
        var about = Generation(await Read(client, "/about"));
        var cache = factory.Services.GetRequiredService<IOutputCacheStore>();

        await cache.EvictByTagAsync("catalog", CancellationToken.None);

        var fresh = await Read(client, "/catalog?view=full");
        Assert.Equal(1, fresh.GetProperty("revision").GetInt32());
        Assert.NotEqual(catalog, Generation(fresh));
        Assert.NotEqual(summary, Generation(await Read(client, "/catalog/summary")));
        Assert.Equal(about, Generation(await Read(client, "/about")));
    }

    [Fact]
    public async Task RevisionPreventsReuseEvenBeforeEviction()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var previous = await Read(client, "/catalog?view=full");

        var catalog = factory.Services.GetRequiredService<Catalog>();
        catalog.ChangePrice(12_900);
        var current = await Read(client, "/catalog?view=full");

        Assert.Equal(12_900, current.GetProperty("priceInCents").GetInt32());
        Assert.NotEqual(Generation(previous), Generation(current));
    }

    [Fact]
    public async Task InvalidPriceDoesNotInvalidateCache()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var before = await Read(client, "/catalog");

        using var invalid = await client.PostAsJsonAsync("/catalog/price", new { priceInCents = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(Generation(before), Generation(await Read(client, "/catalog")));
    }

    private static Task<JsonElement> Read(HttpClient client, string path) =>
        client.GetFromJsonAsync<JsonElement>(path);

    private static long Generation(JsonElement response) => response.GetProperty("generation").GetInt64();
}
