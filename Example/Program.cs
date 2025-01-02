using System.Net;
using LiteHttpListener;
using LiteHttpListener.Attributes;
using LiteHttpListener.Enums;
using LiteHttpListener.Extensions;
using LiteHttpListener.Helper;
using LiteHttpListener.Structs;

namespace Example;

[Route("/")]
[Route("/example")]
public static class ExampleServiceR1
{
    [Route(Methods.Get, "/")]
    [Route(Methods.Get, "index")]
    public static Task Get1(HttpListenerContext context, RouteData routeData)
    {
        context.Response.StatusCode = 302;
        context.Response.RedirectLocation = "/test/redirect";
        return Task.CompletedTask;
    }
    
    [Route(Methods.Get, "/test/{data}")]
    [RouteMeta<string>("metaKey", "this is route meta value")]
    public static async Task Test(HttpListenerContext context, RouteData routeData)
    {
        var metaValue = routeData.Meta.GetMeta<string>("metaKey");
        var query = context.GetQuery();
        var text = "meta:" + metaValue + "\n";
        text += "route data:" + routeData["data"] + "\nquery:";
        text = query.Aggregate(text, (c, p) => c + p.Key + "=" + p.Value + ",");

        var data = System.Text.Encoding.UTF8.GetBytes(text);
        var zipData = await CompressionHelper.GZipCompressAsync(data);
        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.ContentLength64 = zipData.Length;
        context.Response.StatusCode = 200;
        context.Response.Headers.Add("Content-Encoding", "gzip");
        await context.Response.OutputStream.WriteAsync(zipData);
    }
}

[Route("/error")]
public class ExampleServiceR2
{
    private int _count;
    
    [Route(Methods.All, "/")]
    [Route(Methods.All, "/error")]
    public Task Error(HttpListenerContext context, RouteData routeData)
    {
        _count++;
        throw new Exception($"exception count:{_count}");
    }
}

public static class ExampleServiceGuard
{
    [Guard(Guards.StartListenerBefore)]
    public static Task StartListener(HttpListener listener)
    {
        foreach (var listenerPrefix in listener.Prefixes)
        {
            Console.WriteLine($"Listening on {listenerPrefix}");
        }
        return Task.CompletedTask;
    }

    [Guard(Guards.NotFoundRoute)]
    public static async Task NotFoundRoute(HttpListenerContext context)
    {
        var data = "Not Found"u8.ToArray();
        var zipData = await CompressionHelper.GZipCompressAsync(data);
        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.ContentLength64 = zipData.Length;
        context.Response.StatusCode = 404;
        context.Response.Headers.Add("Content-Encoding", "gzip");
        await context.Response.OutputStream.WriteAsync(zipData);
    }

    [Guard(Guards.ServiceException)]
    public static async Task ServiceException(HttpListenerContext context, Exception exception)
    {
        var data = System.Text.Encoding.UTF8.GetBytes(exception.Message);
        var zipData = await CompressionHelper.GZipCompressAsync(data);
        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.ContentLength64 = zipData.Length;
        context.Response.StatusCode = 500;
        context.Response.Headers.Add("Content-Encoding", "gzip");
        await context.Response.OutputStream.WriteAsync(zipData);
    }

    [Guard(Guards.RequestBefore)]
    public static Task<bool> RequestBefore(HttpListenerContext context)
    {
        Console.WriteLine($"{context.Request.HttpMethod} {context.Request.RawUrl}");
        return Task.FromResult(true);
    }
}


public static partial class Program
{
    public static async Task Main(string[] args)
    {
        // var service = new LiteHttpService("http://localhost:5000/");
        // service.RegisterRoute(typeof(ExampleServiceR1));
        // service.RegisterRoute(new ExampleServiceR2());
        // service.RegisterGuard(typeof(ExampleServiceGuard));
        // await service.StartListener();

        var d = Enum.Parse<Methods>("GET", true);
        Console.WriteLine(d);
    }
}
