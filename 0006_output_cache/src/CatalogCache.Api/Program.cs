using System.Globalization;
using Microsoft.AspNetCore.OutputCaching;

const string catalogTag = "catalog";
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<Catalog>();
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy(catalogTag, policy => policy
        .Expire(TimeSpan.FromMinutes(2))
        .Tag(catalogTag)
        .SetVaryByQuery("view")
        .VaryByValue(context => new KeyValuePair<string, string>(
            "revision", context.Features.Get<Product>()!.Revision.ToString(CultureInfo.InvariantCulture))));
});

var app = builder.Build();
var catalog = app.Services.GetRequiredService<Catalog>();
app.Use((context, next) =>
{
    context.Features.Set(catalog.CurrentProduct);
    return next(context);
});
app.UseOutputCache();

app.MapGet("/catalog", (string? view, HttpContext context) =>
{
    if (view is not (null or "brief" or "full"))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["view"] = ["Use brief or full."]
        });
    }

    var product = context.Features.Get<Product>()!;
    return Results.Ok(new
    {
        product.Name,
        product.PriceInCents,
        product.Revision,
        description = view == "full" ? "Compact USB keyboard" : null,
        generation = catalog.NextGeneration()
    });
}).CacheOutput(catalogTag);

app.MapGet("/catalog/summary", (HttpContext context) =>
{
    var product = context.Features.Get<Product>()!;
    return Results.Ok(new { product.PriceInCents, product.Revision, generation = catalog.NextGeneration() });
}).CacheOutput(catalogTag);

app.MapGet("/about", () => Results.Ok(new { generation = catalog.NextGeneration() }))
    .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(2)).Tag("about"));

app.MapPost("/catalog/price", async (PriceChange request, IOutputCacheStore cache) =>
{
    if (request.PriceInCents is < 1 or > 100_000)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["priceInCents"] = ["Use a price between 1 and 100000 cents."]
        });
    }

    var product = catalog.ChangePrice(request.PriceInCents);
    await cache.EvictByTagAsync(catalogTag, CancellationToken.None);
    return Results.Ok(product);
});

app.Run();

public sealed record PriceChange(int PriceInCents);

public partial class Program;
