using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    };
});

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapPost("/reservations/preview", (ReservationRequest request) =>
{
    var errors = new Dictionary<string, string[]>();
    if (string.IsNullOrWhiteSpace(request.Sku))
    {
        errors["sku"] = ["Provide a product SKU."];
    }
    if (request.Quantity is < 1 or > 5)
    {
        errors["quantity"] = ["Quantity must be between 1 and 5."];
    }
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors,
            type: "urn:pathbit:problem:invalid-reservation",
            title: "The reservation is invalid.");
    }

    if (!string.Equals(request.Sku, "keyboard", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            type: "urn:pathbit:problem:product-not-found",
            title: "Product not found.");
    }

    const int availableQuantity = 2;
    if (request.Quantity > availableQuantity)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status409Conflict,
            type: "urn:pathbit:problem:insufficient-stock",
            title: "The requested quantity exceeds available stock.",
            extensions: new Dictionary<string, object?> { ["availableQuantity"] = availableQuantity });
    }

    return Results.Ok(new { sku = "keyboard", quantity = request.Quantity, availableQuantity });
});

app.MapGet("/operations/fail", () =>
{
    throw new InvalidOperationException("The simulated stock backend raised an internal error.");
});

app.Run();

public sealed record ReservationRequest(string? Sku, int Quantity);

public partial class Program;
