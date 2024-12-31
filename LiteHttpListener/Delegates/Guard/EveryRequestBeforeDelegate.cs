namespace LiteHttpListener.Delegates.Guard;

public delegate Task<bool> EveryRequestBeforeDelegate(LiteContext context);
