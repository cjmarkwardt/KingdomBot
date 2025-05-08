namespace Markwardt;

public interface IMap<TKey, T> : IReadOnlyMap<TKey, T>
{
    bool Set(TKey key, T value);
    bool Remove(TKey key);
    void Clear();
}

public static class MapExtensions
{
    public static void Add<TKey, T>(this IMap<TKey, T> map, TKey key, T value)
    {
        if (map.Contains(key))
        {
            throw new ArgumentException("Key already exists", nameof(key));
        }

        map.Set(key, value);
    }
}

public class Map<TKey, T> : IMap<TKey, T>
{
    private readonly Dictionary<Maybe<TKey>, T> map = [];

    public IEnumerable<TKey> Keys => map.Keys.Select(DeconvertKey);
    public IEnumerable<T> Values => map.Values;

    public int Count => map.Count;

    public bool Contains(TKey key)
        => map.ContainsKey(key.Maybe());

    public IEnumerator<KeyValuePair<TKey, T>> GetEnumerator()
        => map.Select(x => new KeyValuePair<TKey, T>(DeconvertKey(x.Key), x.Value)).GetEnumerator();

    public bool Remove(TKey key)
        => map.Remove(ConvertKey(key));

    public bool Set(TKey key, T value)
    {
        if (!Contains(key))
        {
            map[ConvertKey(key)] = value;
            return true;
        }

        return false;
    }

    public bool TryGet(TKey key, [MaybeNullWhen(false)] out T value)
        => map.TryGetValue(ConvertKey(key), out value);

    public void Clear()
        => map.Clear();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private Maybe<TKey> ConvertKey(TKey key)
        => key is null ? new Maybe<TKey>() : key.Maybe();

    private TKey DeconvertKey(Maybe<TKey> key)
        => key.HasValue ? key.Value : default!;
}