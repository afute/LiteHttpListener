using System.Net;

namespace LiteHttpListener;

public sealed class LiteContext(HttpListenerContext context) : IDisposable
{
    public readonly HttpListenerContext Context = context;
    
    public HttpListenerResponse Response => Context.Response;
    public HttpListenerRequest Request => Context.Request;
    
    public void Dispose()
    {
        Context.Response.Close();
    }
    
    
}
