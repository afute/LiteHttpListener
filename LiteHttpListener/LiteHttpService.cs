using System.Net;
using LiteHttpListener.Extensions;

namespace LiteHttpListener;

public class LiteHttpService : LiteHttpServiceRouter
{
    private readonly HttpListener _listener;
    private static CancellationToken _token = CancellationToken.None;

    public LiteHttpService(string[] prefixes)
    {
        _listener = new HttpListener();
        foreach (var prefix in prefixes) _listener.Prefixes.Add(prefix);
    }

    public LiteHttpService(string prefixes) : this([prefixes])
    {
    }

    public async Task StartListener(CancellationToken? token = null)
    {
        _token = token ?? CancellationToken.None;

        if (StartListenerBeforeDelegate is not null)
        {
            await StartListenerBeforeDelegate.Invoke(_listener);
        }
        if (!_listener.IsListening) _listener.Start();

        while (!_token.IsCancellationRequested)
        {
            var context = await _listener.GetContextAsync();
            _ = ListenerTask(context);
        }

        _listener.Stop();
    }

    private async Task ListenerTask(HttpListenerContext context)
    {
        try
        {
            if (RequestBeforeDelegate is not null)
            {
                var next = await RequestBeforeDelegate.Invoke(context);
                if (!next) goto close;
            }

            var method = context.GetMethod();
            var routeRaw = context.GetRouteRaw();
            
            if (routeRaw is null)
            {
                if (NotFoundRouteDelegate is not null)
                {
                    await NotFoundRouteDelegate.Invoke(context);
                }
                goto close;
            }
            
            var data = MatchRoute(method, routeRaw.Value);

            if (data is null)
            {
                if (NotFoundRouteDelegate is not null)
                {
                    await NotFoundRouteDelegate.Invoke(context);
                }
                goto close;
            }

            await data.Item1.Invoke(context, data.Item2);
        }
        catch (Exception exception)
        {
            if (ServiceExceptionDelegate is not null)
            {
                await ServiceExceptionDelegate.Invoke(context, exception);
            }
        }
        
        close:
        context.Response.Close();
    }
}
