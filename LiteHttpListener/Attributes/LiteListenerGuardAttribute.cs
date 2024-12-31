using LiteHttpListener.Enums;

namespace LiteHttpListener.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class LiteListenerGuardAttribute(LiteGuard guard) : Attribute
{
    public readonly LiteGuard Guard = guard;
}
