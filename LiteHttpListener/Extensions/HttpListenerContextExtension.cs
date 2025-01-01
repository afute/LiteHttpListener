using System.Net;
using System.Web;
using LiteHttpListener.Enums;
using LiteHttpListener.Helper;
using LiteHttpListener.Structs;

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
        return MethodsHelper.GetMethodEnum(context.Request.HttpMethod);
    }
}
