using System.Threading.Channels;

public sealed record Job(Guid Id, int Units);

public sealed class JobQueue(int capacity)
{
    private readonly Channel<Job> channel = Channel.CreateBounded<Job>(new BoundedChannelOptions(capacity)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.Wait
    });

    public bool TryEnqueue(Job job) => channel.Writer.TryWrite(job);

    public IAsyncEnumerable<Job> ReadAllAsync(CancellationToken cancellationToken) =>
        channel.Reader.ReadAllAsync(cancellationToken);
}
