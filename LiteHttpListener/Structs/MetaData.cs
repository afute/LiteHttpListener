using System.Reflection;
using LiteHttpListener.Attributes;

namespace LiteHttpListener.Structs;

public readonly struct MetaData(MethodInfo methodInfo)
{
    public T? GetMeta<T>(string key)
    {
        var metaData = methodInfo
            .GetCustomAttributes<LiteRouteMetaAttribute<T>>()
            .FirstOrDefault(x => x.Key == key);
        return metaData == null ? default : metaData.Value;
    }
}
