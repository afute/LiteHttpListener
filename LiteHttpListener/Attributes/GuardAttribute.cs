using LiteHttpListener.Enums;

namespace LiteHttpListener.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class GuardAttribute(Guards guards) : Attribute
{
    public readonly Guards Guards = guards;
}
