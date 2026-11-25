public interface IJobProcessor
{
    Task ProcessAsync(Job job, CancellationToken cancellationToken);
}

public sealed class SimulatedJobProcessor(ILogger<SimulatedJobProcessor> logger) : IJobProcessor
{
    public async Task ProcessAsync(Job job, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(job.Units * 250), cancellationToken);
        logger.LogInformation("Completed job {JobId} with {Units} work units.", job.Id, job.Units);
    }
}

public sealed class JobWorker(
    JobQueue queue,
    IJobProcessor processor,
    ILogger<JobWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await processor.ProcessAsync(job, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception error)
            {
                logger.LogError(error, "Job {JobId} failed.", job.Id);
            }
        }
    }
}
