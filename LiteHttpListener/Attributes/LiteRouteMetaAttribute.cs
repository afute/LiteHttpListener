namespace LiteHttpListener.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class LiteRouteMetaAttribute<T>(string key, T? value) : Attribute
{
    public readonly string Key = key;
    public readonly T? Value = value;
}
