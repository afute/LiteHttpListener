using System.Reflection;
using LiteHttpListener.Attributes;
using LiteHttpListener.Enums;
using LiteHttpListener.Structs;

namespace LiteHttpListener;

using RouteDelegate = Tuple<RouteAttribute, MethodData>;

public abstract class LiteHttpServiceRouter : LiteHttpServiceGuard
{
    private readonly List<RouteDelegate> _staticRoutes = [];
    private readonly List<RouteDelegate> _dynamicRoutes = [];

    private void RegisterRoute(Type type, object? obj)
    {
        var baseAttrs = type.GetCustomAttributes<RouteAttribute>();
        var baseRoutes = baseAttrs.Select(x => x.Route).ToArray();
        baseRoutes = baseRoutes.Length == 0 ? [""] : baseRoutes;

        var methodTuples = from info in type.GetMethods()
            let attrs = info.GetCustomAttributes<RouteAttribute>()
            from attribute in attrs
            from baseRoute in baseRoutes
            select Tuple.Create(info, attribute, baseRoute);

        foreach (var (methodInfo, attribute, baseRoute) in methodTuples)
        {
            var method = attribute.Method;
            var route = baseRoute + attribute.Route;
            var attr = new RouteAttribute(method, route);
            var fn = new MethodData(methodInfo, obj);
            var data = Tuple.Create(attr, fn);
            (route.IsDynamic ? _dynamicRoutes : _staticRoutes).Add(data);
        }

        _staticRoutes.Sort(SortHandler);
        _dynamicRoutes.Sort(SortHandler);
    }

    private static int SortHandler(RouteDelegate a, RouteDelegate b)
    {
        return b.Item1.Route.Length - a.Item1.Route.Length;
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

    internal Tuple<MethodData.ServiceFn, RouteData>? MatchRoute(Methods method, RouteRaw rawRoute)
    {
        foreach (var staticRouteData in _staticRoutes)
        {
            if (!staticRouteData.Item1.Method.HasFlag(method)) continue;
            if (!staticRouteData.Item1.MatchRoute(rawRoute, out _)) continue;
            var routeData = new RouteData(null, staticRouteData.Item2.Meta);
            return Tuple.Create(staticRouteData.Item2.Fn, routeData);
        }

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var dynamicRouteData in _dynamicRoutes)
        {
            if (!dynamicRouteData.Item1.Method.HasFlag(method)) continue;
            if (!dynamicRouteData.Item1.MatchRoute(rawRoute, out var ps)) continue;
            var routeData = new RouteData(ps, dynamicRouteData.Item2.Meta);
            return Tuple.Create(dynamicRouteData.Item2.Fn, routeData);
        }

        return null;
    }
}
