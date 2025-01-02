using System.Text.RegularExpressions;
using LiteHttpListener.Enums;
using LiteHttpListener.Helper;
using LiteHttpListener.Structs;

namespace LiteHttpListener.Attributes;

using RouteParameters = IReadOnlyDictionary<string, string>;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed partial class RouteAttribute : Attribute
{
    [GeneratedRegex(@"\{(\w+)(\*)?\}", RegexOptions.Compiled)]
    private static partial Regex DynamicRouteRegex();
    
    public readonly Methods Method;
    public readonly RouteRaw Route;
    
    public RouteAttribute(RouteRaw route)
    {
        Route = route;
        Method = default;
    }
    
    public RouteAttribute(string route)
    {
        Route = (RouteRaw)route;
        Method = default;
    }
    
    public RouteAttribute(Methods method, RouteRaw route)
    {
        Method = method;
        Route = route;
    }
    
    public RouteAttribute(Methods method, string route)
    {
        Method = method;
        Route = (RouteRaw)route;
    }
    
    public RouteAttribute(string method, string route)
    {
        Method = Enum.Parse<Methods>(method, true);
        Route = (RouteRaw)route;
    }
    
    public RouteAttribute(string method, RouteRaw route)
    {
        Method = Enum.Parse<Methods>(method, true);
        Route = route;
    }
    
    public bool MatchRoute(RouteRaw raw, out RouteParameters? parameters)
    {
        parameters = null;
        if (raw.Length < Route.Length) return false;
        if (!Route.IsDynamic) return raw.Route == Route.Route;
        
        var reg = DynamicRouteRegex();
        var routeParameters = new Dictionary<string, string>();
        var isMatch = true;

        var lastMatch = reg.Match(Route[^1]);
        if (lastMatch.Success)
        {
            var paramName = lastMatch.Groups[1].Value;
            var data = string.Join("/", raw[^1..]);
            routeParameters.Add(paramName, data);
        }
        
        var route1 = Route[..^1];
        var route2 = raw[..route1.Length];
        
        for (var i = 0; i < route1.Length; i++)
        {
            if (Route[i] == route2[i]) continue;
            var match = reg.Match(Route[i]);
            isMatch = match.Success;
            if (!isMatch) break;
            var paramName = match.Groups[1].Value;
            routeParameters.Add(paramName, route2[i]);
        }

        if (!isMatch || routeParameters.Count == 0)
        {
            routeParameters.Clear();
            parameters = null;
            return isMatch;
        }

        parameters = routeParameters;
        return isMatch;
    }
}
