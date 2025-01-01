using System.Net;
using System.Reflection;

namespace LiteHttpListener.Structs;

internal readonly struct MethodData
{
    internal delegate Task ServiceFn(HttpListenerContext context, RouteData routeData);
    
    internal readonly ServiceFn Fn;
    
    internal readonly MetaData Meta;

    internal MethodData(MethodInfo methodInfo, object? obj)
    {
        var target = methodInfo.IsStatic ? null : obj;
        Fn = methodInfo.CreateDelegate<ServiceFn>(target);
        Meta = new MetaData(methodInfo);
    }
}
