public sealed record Product(string Name, int PriceInCents, long Revision);

public sealed class Catalog
{
    private readonly object updateLock = new();
    private Product product = new("Keyboard", 9_900, 1);
    private long generation;

    public Product CurrentProduct => Volatile.Read(ref product);

    public long NextGeneration() => Interlocked.Increment(ref generation);

    public Product ChangePrice(int priceInCents)
    {
        lock (updateLock)
        {
            var updated = product with { PriceInCents = priceInCents, Revision = product.Revision + 1 };
            Volatile.Write(ref product, updated);
            return updated;
        }
    }
}
