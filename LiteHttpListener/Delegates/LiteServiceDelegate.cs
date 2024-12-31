using LiteHttpListener.Structs;

namespace LiteHttpListener.Delegates;

public delegate Task LiteServiceDelegate(LiteContext context, RouteData route);
