using System.Text.RegularExpressions;
using LiteHttpListener.Enums;

namespace LiteHttpListener.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed partial class LiteRouteAttribute : Attribute
{
    [GeneratedRegex(@"\{(\w+)(\*)?\}", RegexOptions.Compiled)]
    private static partial Regex DynamicRouteRegex();
    
    public readonly LiteMethod Method;
    public readonly string Route;
    public readonly bool DynamicRoute;
    
    public LiteRouteAttribute(LiteMethod method, string path)
    {
        Method = method;
        if (Method == 0) throw new Exception("Invalid method");
        var parts = path.Split("/").Where(x => x != "").ToArray();
        Route = parts.Length == 0 ? "" : "/" + string.Join("/", parts);
        DynamicRoute = Route.Contains('{') || Route.Contains('}');
        if (DynamicRoute && !DynamicRouteRegex().Match(Route).Success)
        {
            throw new Exception("Invalid dynamic route");
        }
    }
}
