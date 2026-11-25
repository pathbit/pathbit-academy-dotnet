using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BoundedJobs.Tests;

public sealed class JobContracts
{
    [Fact]
    public async Task FullQueueRejectsNewJobsAndKeepsAcceptedJobsInOrder()
    {
        var processor = new ControlledProcessor();
        using var factory = new WebApplicationFactory<Program>();
        using var configured = WithProcessor(factory, processor);
        using var client = configured.CreateClient();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var token = timeout.Token;

        var first = await Enqueue(client, token);
        Assert.Equal(first, await processor.Started.Reader.ReadAsync(token));
        var second = await Enqueue(client, token);
        var third = await Enqueue(client, token);
        using var overloaded = await client.PostAsJsonAsync("/jobs", new { units = 1 }, token);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, overloaded.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(1), overloaded.Headers.RetryAfter?.Delta);
        Assert.Equal("application/problem+json", overloaded.Content.Headers.ContentType?.MediaType);
        var problem = await overloaded.Content.ReadFromJsonAsync<JsonElement>(token);
        Assert.Equal("urn:pathbit:problem:job-queue-full", problem.GetProperty("type").GetString());

        processor.ReleaseFirst.TrySetResult();
        Assert.Equal(second, await processor.Started.Reader.ReadAsync(token));
        Assert.Equal(third, await processor.Started.Reader.ReadAsync(token));
        var next = await Enqueue(client, token);
        Assert.Equal(next, await processor.Started.Reader.ReadAsync(token));
    }

    [Fact]
    public async Task WorkerContinuesWhenAJobFails()
    {
        var processor = new ControlledProcessor(failFirst: true);
        using var factory = new WebApplicationFactory<Program>();
        using var configured = WithProcessor(factory, processor);
        using var client = configured.CreateClient();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var token = timeout.Token;

        var first = await Enqueue(client, token);
        Assert.Equal(first, await processor.Started.Reader.ReadAsync(token));
        var next = await Enqueue(client, token);
        processor.ReleaseFirst.TrySetResult();
        Assert.Equal(next, await processor.Started.Reader.ReadAsync(token));
    }

    [Fact]
    public async Task InvalidInputDoesNotCreateAJob()
    {
        var processor = new ControlledProcessor();
        using var factory = new WebApplicationFactory<Program>();
        using var configured = WithProcessor(factory, processor);
        using var client = configured.CreateClient();

        using var invalid = await client.PostAsJsonAsync("/jobs", new { units = 6 });

        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.False(processor.Started.Reader.TryRead(out _));
    }

    private static WebApplicationFactory<Program> WithProcessor(
        WebApplicationFactory<Program> factory, IJobProcessor processor) =>
        factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton(processor)));

    private static async Task<Guid> Enqueue(HttpClient client, CancellationToken token)
    {
        using var response = await client.PostAsJsonAsync("/jobs", new { units = 1 }, token);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var job = await response.Content.ReadFromJsonAsync<JsonElement>(token);
        return job.GetProperty("id").GetGuid();
    }

    private sealed class ControlledProcessor(bool failFirst = false) : IJobProcessor
    {
        private bool first = true;

        public Channel<Guid> Started { get; } = Channel.CreateUnbounded<Guid>();
        public TaskCompletionSource ReleaseFirst { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task ProcessAsync(Job job, CancellationToken cancellationToken)
        {
            await Started.Writer.WriteAsync(job.Id, cancellationToken);
            if (first)
            {
                first = false;
                await ReleaseFirst.Task.WaitAsync(cancellationToken);
                if (failFirst)
                {
                    throw new InvalidOperationException("The first test job failed.");
                }
            }
        }
    }
}
