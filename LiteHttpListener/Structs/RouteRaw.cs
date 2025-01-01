using System.Text.RegularExpressions;

namespace LiteHttpListener.Structs;

public readonly partial struct RouteRaw : IEquatable<RouteRaw>
{
    [GeneratedRegex(@"\{(\w+)(\*)?\}", RegexOptions.Compiled)]
    private static partial Regex DynamicRouteRegex();
    
    private readonly string[] _routeParts;
    
    public string Route { get; init; }
    
    public bool IsDynamic { get; init; }
    
    public string this[int index] => _routeParts[index];
    
    public string[] this[Range range] => _routeParts[range];

    public int Length => _routeParts.Length;

    public RouteRaw(string path)
    {
        var parts = path.Split("?")[0].Split("/");
        _routeParts = parts.Where(x => x != "").ToArray();
        Route = string.Join("/", _routeParts);
        IsDynamic = DynamicRouteRegex().Match(Route).Success;
    }

    public RouteRaw(Uri uri) : this(uri.AbsolutePath) { }
    
    public static RouteRaw operator +(RouteRaw a, RouteRaw b)
    {
        return new RouteRaw(string.Join("/", a.Route, b.Route));
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is RouteRaw routeRaw)
        {
            return Route == routeRaw.Route;
        }
        return false;
    }
    
    public bool Equals(RouteRaw routeRaw)
    {
        return Route == routeRaw.Route;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Route);
    }
    
    public static bool operator ==(RouteRaw a, RouteRaw b)
    {
        return a.Route == b.Route;
    }
    
    public static bool operator !=(RouteRaw a, RouteRaw b)
    {
        return a.Route != b.Route;
    }
    
    public static implicit operator string(RouteRaw raw) => raw.Route;
    public static implicit operator RouteRaw(string raw) => new(raw);
}
