using LiteHttpListener;
using LiteHttpListener.Attributes;
using LiteHttpListener.Enums;
using LiteHttpListener.Helper;
using LiteHttpListener.Structs;

namespace Example;

public static class ExampleServiceR1
{
    [LiteRoute(LiteMethod.Get, "/")]
    [LiteRoute(LiteMethod.Get, "/index")]
    public static async Task Get1(LiteContext context, RouteData routeData)
    {
        var data = "wow"u8.ToArray();
        var zipData = await CompressionHelper.GZipCompressAsync(data);
        context.Response.StatusCode = 200;
        context.Response.ContentLength64 = zipData.Length;
        context.Response.ContentType = "text/plain";
        context.Response.Headers.Add("Content-Encoding", "gzip");
        await context.Response.OutputStream.WriteAsync(zipData);
    }

    [LiteRoute(LiteMethod.Get, "/error")]
    [LiteRouteMeta<string>("metaKey", "this is route meta value")]
    public static Task Error(LiteContext context, RouteData routeData)
    {
        throw new Exception(routeData.Meta.GetMeta<string>("metaKey"));
    }
}

[LiteRouteBase("/test")]
[LiteRouteBase("/test2")]
public class ExampleServiceR2
{
    private int _count;

    [LiteRoute(LiteMethod.Get, "/add/{name}")]
    public async Task AddCount(LiteContext context, RouteData routeData)
    {
        _count++;
        var name = routeData.Param?["name"];
        var data = System.Text.Encoding.UTF8.GetBytes($"{name}:{_count}");
        var zipData = await CompressionHelper.GZipCompressAsync(data);
        context.Response.StatusCode = 200;
        context.Response.ContentLength64 = zipData.Length;
        context.Response.ContentType = "text/plain";
        context.Response.Headers.Add("Content-Encoding", "gzip");
        await context.Response.OutputStream.WriteAsync(zipData);
    }
}

public static class Guard
{
    [LiteListenerGuard(LiteGuard.NotFoundRoute)]
    public static Task NotFoundRoute(LiteContext context)
    {
        context.Response.StatusCode = 404;
        Console.WriteLine($"{context.Request.RawUrl} not found");
        return Task.CompletedTask;
    }
    
    [LiteListenerGuard(LiteGuard.EveryRequestBefore)]
    public static Task<bool> EveryRequestBefore(LiteContext context)
    {
        Console.WriteLine($"{context.Request.HttpMethod} {context.Request.RawUrl}");
        return Task.FromResult(true);
    }
    
    [LiteListenerGuard(LiteGuard.ServiceException)]
    public static async Task ServiceException(LiteContext context, Exception exception)
    {
        var data = System.Text.Encoding.UTF8.GetBytes(exception.Message);
        var zipData = await CompressionHelper.GZipCompressAsync(data);
        context.Response.StatusCode = 500;
        context.Response.ContentLength64 = zipData.Length;
        context.Response.ContentType = "text/plain";
        context.Response.Headers.Add("Content-Encoding", "gzip");
        await context.Response.OutputStream.WriteAsync(zipData);
        Console.WriteLine($"{exception.Message}");
    }
}

public static class Program
{
    public static async Task Main(string[] args)
    {
        const string prefixes = "http://127.0.0.1:5000/";
        var service = new LiteHttpService(prefixes);
        
        service.RegisterRoute(typeof(ExampleServiceR1));
        service.RegisterRoute(new ExampleServiceR2());
        
        service.RegisterGuard(typeof(Guard));

        Console.WriteLine($"Listening on {prefixes}");

        await service.StartListener();
    }
}
