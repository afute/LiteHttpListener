namespace LiteHttpListener.Enums;

[Flags]
public enum LiteMethod
{
    Get = 1 << 0,
    Post = 1 << 1,
    Put = 1 << 2,
    Delete = 1 << 3,
    Head = 1 << 4,
    Options = 1 << 5,
    Trace = 1 << 6,
    Patch = 1 << 7,
    Connect = 1 << 8
}
