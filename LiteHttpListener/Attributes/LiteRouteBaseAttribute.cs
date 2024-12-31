namespace LiteHttpListener.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class LiteRouteBaseAttribute : Attribute
{
    public readonly string Route;

    public LiteRouteBaseAttribute(string path)
    {
        var parts = path.Split("/").Where(x => x != "").ToArray();
        Route = parts.Length == 0 ? "" : "/" + string.Join("/", parts);
        if (Route.Contains('{') || Route.Contains('}'))
        {
            throw new Exception("Routes cannot be dynamic");
        }
    }
}
