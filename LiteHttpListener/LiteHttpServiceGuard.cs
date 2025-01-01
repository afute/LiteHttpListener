using System.Net;
using System.Reflection;
using LiteHttpListener.Attributes;
using LiteHttpListener.Enums;

namespace LiteHttpListener;

public abstract class LiteHttpServiceGuard
{
    protected delegate Task StartListenerBefore(HttpListener listener);

    protected delegate Task NotFoundRoute(HttpListenerContext context);

    protected delegate Task ServiceException(HttpListenerContext context, Exception exception);

    protected delegate Task<bool> RequestBefore(HttpListenerContext context);

    protected StartListenerBefore? StartListenerBeforeDelegate;
    protected NotFoundRoute? NotFoundRouteDelegate;
    protected ServiceException? ServiceExceptionDelegate;
    protected RequestBefore? RequestBeforeDelegate;

    private void RegisterGuard(Type type, object? obj)
    {
        var data = from methodInfo in type.GetMethods()
            let attribute = methodInfo.GetCustomAttribute<GuardAttribute>()
            where attribute != null
            select Tuple.Create(methodInfo, attribute);
        foreach (var (methodInfo, attribute) in data)
        {
            var target = methodInfo.IsStatic ? null : obj;
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (attribute.Guards == Guards.StartListenerBefore)
            {
                StartListenerBeforeDelegate = methodInfo.CreateDelegate<StartListenerBefore>(target);
            }
            else if (attribute.Guards == Guards.NotFoundRoute)
            {
                NotFoundRouteDelegate = methodInfo.CreateDelegate<NotFoundRoute>(target);
            }
            else if (attribute.Guards == Guards.ServiceException)
            {
                ServiceExceptionDelegate = methodInfo.CreateDelegate<ServiceException>(target);
            }
            else if (attribute.Guards == Guards.RequestBefore)
            {
                RequestBeforeDelegate = methodInfo.CreateDelegate<RequestBefore>(target);
            }
        }
    }
    
    public void RegisterGuard(object obj)
    {
        var type = obj.GetType();
        RegisterGuard(type, obj);
    }

    public void RegisterGuard(Type type)
    {
        RegisterGuard(type, null);
    }
}
