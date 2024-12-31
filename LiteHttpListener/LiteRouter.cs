using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using LiteHttpListener.Attributes;
using LiteHttpListener.Delegates;
using LiteHttpListener.Enums;
using LiteHttpListener.Helper;
using LiteHttpListener.Structs;

namespace LiteHttpListener;

using RoutesType = Dictionary<Tuple<LiteMethod, string>, Tuple<LiteServiceDelegate, MethodInfo>>;

public abstract partial class LiteRouter
{
    [GeneratedRegex(@"\{(\w+)(\*)?\}", RegexOptions.Compiled)]
    private static partial Regex DynamicRouteRegex();
    
    private readonly RoutesType _staticRoutes = new();
    private readonly RoutesType _dynamicRoutes = new();
    
    private void RegisterRoute(Type type, object? obj)
    {
        var baseAttrs = type.GetCustomAttributes<LiteRouteBaseAttribute>();
        var baseRoutes = baseAttrs.Select(x => x.Route).ToArray();
        baseRoutes = baseRoutes.Length == 0 ? [""] : baseRoutes;
        
        var methodTuples = from info in type.GetMethods()
            let attrs = info.GetCustomAttributes<LiteRouteAttribute>()
            from attribute in attrs
            from baseRoute in baseRoutes 
            select Tuple.Create(info, attribute, baseRoute);
        
        foreach (var (methodInfo, attribute, baseRoute) in methodTuples)
        {
            var method = attribute.Method;
            var route = baseRoute + attribute.Route;
            route = route == "" ? "/" : route;
            
            var target = methodInfo.IsStatic ? null : obj;
            var handler = methodInfo.CreateDelegate<LiteServiceDelegate>(target);
            var key = Tuple.Create(method, route);
            
            if (!attribute.DynamicRoute) _staticRoutes.Add(key, Tuple.Create(handler, methodInfo));
            else _dynamicRoutes.Add(key, Tuple.Create(handler, methodInfo));
        }
    }
    
    public void RegisterRoute(object obj)
    {
        var type = obj.GetType();
        RegisterRoute(type, obj);
    }
    
    public void RegisterRoute(Type type)
    {
        RegisterRoute(type, null);
    }
}

public abstract partial class LiteRouter
{
    protected Tuple<LiteServiceDelegate, RouteData>? Matching(string rawMethod, Uri rawUri)
    {
        var parts = rawUri.AbsolutePath.Split("/").Where(x => x != "").ToArray();
        var path = parts.Length == 0 ? "/" : "/" + string.Join("/", parts);
        var method = LiteMethodHelper.GetLiteMethod(rawMethod);
        var collection = HttpUtility.ParseQueryString(rawUri.Query);
        var query = collection.AllKeys.OfType<string>()
            .ToDictionary(key => key, key => collection[key]);
        query = query.Count == 0 ? null : query;
        
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var staticRoute in _staticRoutes)
        {
            if (!staticRoute.Key.Item1.HasFlag(method)) continue;
            if (path != staticRoute.Key.Item2) continue;
            var data = new RouteData
            {
                RawMethod = rawMethod,
                RawPath = rawUri.PathAndQuery,
                Method = method,
                Path = path,
                Meta = new MetaData(staticRoute.Value.Item2),
                Query = query,
                Param = null,
            };
            return Tuple.Create(staticRoute.Value.Item1, data);
        }
        
        var paramRegex = DynamicRouteRegex();
        var param = new Dictionary<string, string>();

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var dynamicRoute in _dynamicRoutes)
        {
            if (!dynamicRoute.Key.Item1.HasFlag(method)) continue;
            
            var routeParts = dynamicRoute.Key.Item2.Split("/");
            var pathParts = path.Split("/");

            var isMatch = true;
            
            for (int i = 0, j = 0; i < routeParts.Length && j < pathParts.Length; i++, j++)
            {
                var match = paramRegex.Match(routeParts[i]);
                if (match.Success)
                {
                    var paramName = match.Groups[1].Value;
                    if (match.Groups[2].Success)
                    {
                        param[paramName] = string.Join("/", pathParts, j, pathParts.Length - j);
                        j = pathParts.Length; // consume all remaining path parts
                    }
                    else param[paramName] = pathParts[j];
                }
                else if (routeParts[i] != pathParts[j])
                {
                    isMatch = false;
                    param.Clear();
                    break;
                }
            }
            if (isMatch)
            {
                var data = new RouteData
                {
                    RawMethod = rawMethod,
                    RawPath = rawUri.PathAndQuery,
                    Method = method,
                    Path = path,
                    Meta = new MetaData(dynamicRoute.Value.Item2),
                    Query = query,
                    Param = param,
                };
                return Tuple.Create(dynamicRoute.Value.Item1, data);
            }
        }
        return null;
    }
}
