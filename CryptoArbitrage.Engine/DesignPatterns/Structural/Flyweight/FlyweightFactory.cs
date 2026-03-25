using System.Collections.ObjectModel;

namespace CryptoArbitrage.Engine.DesignPatterns.Structural.Flyweight;

/// <summary>
/// FlyweightFactory caches flyweights by repeating symbol state.
/// </summary>
public class FlyweightFactory
{
    private readonly Dictionary<string, Flyweight> cache = new(StringComparer.OrdinalIgnoreCase);

    public Flyweight GetFlyweight(string repeatingState)
    {
        var key = Normalize(repeatingState);

        if (!cache.TryGetValue(key, out var flyweight))
        {
            flyweight = new Flyweight(key);
            cache[key] = flyweight;
        }

        return flyweight;
    }

    public bool HasFlyweight(string repeatingState)
    {
        return cache.ContainsKey(Normalize(repeatingState));
    }

    public int CacheSize => cache.Count;

    public IReadOnlyDictionary<string, Flyweight> GetCachedFlyweights()
    {
        return new ReadOnlyDictionary<string, Flyweight>(cache);
    }

    private static string Normalize(string repeatingState)
    {
        return repeatingState.Trim().ToUpperInvariant();
    }
}
