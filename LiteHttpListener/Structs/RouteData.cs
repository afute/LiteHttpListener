namespace LiteHttpListener.Structs;

using RouteParameters = IReadOnlyDictionary<string, string>;

public readonly struct RouteData(RouteParameters? parameters, MetaData metaData)
{
    public string? this[string key]
    {
        get
        {
            if (parameters is null) return null;
            parameters.TryGetValue(key, out var value);
            return value;
        }
    }
    
    public readonly MetaData Meta = metaData;
}
