using LiteHttpListener.Enums;

namespace LiteHttpListener.Helper;

public static class LiteMethodHelper
{
    public static LiteMethod GetLiteMethod(string method)
    {
        const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
        
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var value in Enum.GetValues<LiteMethod>())
        {
            if (method.Equals(value.ToString(), comparison))
            {
                return value;
            }
        }
        return 0;
    }
}
