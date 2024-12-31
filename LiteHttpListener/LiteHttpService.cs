using System.Net;
using System.Reflection;
using LiteHttpListener.Attributes;
using LiteHttpListener.Delegates.Guard;
using LiteHttpListener.Enums;

namespace LiteHttpListener;

public partial class LiteHttpService : LiteRouter
{
    private readonly HttpListener _listener;
    private static CancellationToken _token = CancellationToken.None;
    
    public LiteHttpService(string[] prefixes)
    {
        _listener = new HttpListener();
        foreach (var prefix in prefixes) _listener.Prefixes.Add(prefix);
    }
    
    public LiteHttpService(string prefixes) : this([prefixes]) {}
    
    public async Task StartListener(CancellationToken? token = null)
    {
        _token = token ?? CancellationToken.None;
        _listener.Start();
        
        while (!_token.IsCancellationRequested)
        {
            var context = await _listener.GetContextAsync();
            _ = ListenerTask(context);
        }
        
        _listener.Stop();
    }
    
    private async Task ListenerTask(HttpListenerContext context)
    {
        using var liteContext = new LiteContext(context);
        var next = true;
        if (_guardEveryRequestBeforeDelegate != null)
        {
            next = await _guardEveryRequestBeforeDelegate.Invoke(liteContext);
        }

        if (!next)
        {
            liteContext.Response.Close();
            return;
        }

        try
        {
            var method = liteContext.Request.HttpMethod;
            var url = liteContext.Request.Url;
            if (url == null)
            {
                if (_guardNotFoundRouteDelegate != null)
                {
                    await _guardNotFoundRouteDelegate.Invoke(liteContext);
                }

                goto close;
            }

            var matchData = Matching(method, url);
            if (matchData == null)
            {
                if (_guardNotFoundRouteDelegate != null)
                {
                    await _guardNotFoundRouteDelegate.Invoke(liteContext);
                }
                
                goto close;
            }

            await matchData.Item1.Invoke(liteContext, matchData.Item2);
        }
        catch (Exception exception)
        {
            if (_guardServiceExceptionDelegate != null)
            {
                await _guardServiceExceptionDelegate.Invoke(liteContext, exception);
            }
        }
        
        close:
        liteContext.Response.Close();
    }
}

public partial class LiteHttpService
{
    private NotFoundRouteDelegate? _guardNotFoundRouteDelegate = null;
    private ServiceExceptionDelegate? _guardServiceExceptionDelegate = null;
    private EveryRequestBeforeDelegate? _guardEveryRequestBeforeDelegate = null;
    
    private void RegisterGuard(Type type, object? obj)
    {
        var data = from methodInfo in type.GetMethods()
            let attr = methodInfo.GetCustomAttribute<LiteListenerGuardAttribute>()
            where attr != null
            select Tuple.Create(methodInfo, attr.Guard);
        foreach (var (methodInfo, guard) in data)
        {
            var target = methodInfo.IsStatic ? null : obj;
            
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (guard == LiteGuard.NotFoundRoute)
            {
                var fn1 = methodInfo.CreateDelegate<NotFoundRouteDelegate>(target);
                _guardNotFoundRouteDelegate = fn1;
            }
            else if (guard == LiteGuard.ServiceException)
            {
                var fn2 = methodInfo.CreateDelegate<ServiceExceptionDelegate>(target);
                _guardServiceExceptionDelegate = fn2;
            }
            else if (guard == LiteGuard.EveryRequestBefore)
            {
                var fn3 = methodInfo.CreateDelegate<EveryRequestBeforeDelegate>(target);
                _guardEveryRequestBeforeDelegate = fn3;
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
