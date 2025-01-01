using LiteHttpListener.Enums;

namespace LiteHttpListener.Helper;

public static class MethodsHelper
{
    public static Methods GetMethodEnum(string method)
    {
        const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
        
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var value in Enum.GetValues<Methods>())
        {
            if (method.Equals(value.ToString(), comparison))
            {
                return value;
            }
        }
        return default;
    }
}
