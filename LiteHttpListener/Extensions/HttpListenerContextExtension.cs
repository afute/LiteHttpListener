using System.Net;
using System.Web;
using LiteHttpListener.Enums;
using LiteHttpListener.Helper;
using LiteHttpListener.Structs;
using Newtonsoft.Json;

namespace LiteHttpListener.Extensions;

public static class HttpListenerContextExtension
{
    public static IReadOnlyDictionary<string, string?> GetQuery(this HttpListenerContext context)
    {
        var query = context.Request.Url?.Query ?? string.Empty;
        var nv = HttpUtility.ParseQueryString(query);
        return nv.AllKeys.OfType<string>().ToDictionary(key => key, key => nv[key]);
    }
    
    public static RouteRaw? GetRouteRaw(this HttpListenerContext context)
    {
        var url = context.Request.Url;
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (url is null) return null;
        return new RouteRaw(url);
    }

    public static Methods GetMethod(this HttpListenerContext context)
    {
        return Enum.Parse<Methods>(context.Request.HttpMethod, true);
    }

    public static async Task SendJson(this HttpListenerContext context, object obj, string charset = "utf-8")
    {
        var json = JsonConvert.SerializeObject(obj);
        var jsonData = System.Text.Encoding.UTF8.GetBytes(json);
        
        var acceptEncoding = context.Request.Headers["Accept-Encoding"] ?? "";
        var splits = from v in acceptEncoding.Split(",") select v.Trim();
        
        var data = jsonData; 
        if (splits.Contains("gzip"))
        {
            data = await CompressionHelper.GZipCompressAsync(jsonData);
            context.Response.Headers.Add("Content-Encoding", "gzip");
        }
        context.Response.Headers.Add("Content-Type", $"application/json; charset={charset}");
        context.Response.ContentLength64 = data.Length;
        await context.Response.OutputStream.WriteAsync(data);
    }
}
