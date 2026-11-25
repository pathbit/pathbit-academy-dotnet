var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(new JobQueue(capacity: 2));
builder.Services.AddSingleton<IJobProcessor, SimulatedJobProcessor>();
builder.Services.AddHostedService<JobWorker>();

var app = builder.Build();
app.MapPost("/jobs", (JobRequest request, JobQueue queue, HttpContext context) =>
{
    if (request.Units is < 1 or > 5)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["units"] = ["Units must be between 1 and 5."]
        });
    }

    var job = new Job(Guid.NewGuid(), request.Units);
    if (!queue.TryEnqueue(job))
    {
        context.Response.Headers.RetryAfter = "1";
        return Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            type: "urn:pathbit:problem:job-queue-full",
            title: "The job queue is full.");
    }

    return Results.Accepted(value: new { id = job.Id });
});

app.Run();

public sealed record JobRequest(int Units);

public partial class Program;
