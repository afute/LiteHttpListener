using LiteHttpListener.Delegates;
using LiteHttpListener.Enums;

namespace LiteHttpListener.Structs;

public readonly struct RouteData
{
    public readonly string RawMethod { get; init; }
    public required string RawPath { get; init; }
    
    public required LiteMethod Method {get; init;}
    
    public required string Path {get; init;}
    public required MetaData Meta { get; init; }
    
    public required IReadOnlyDictionary<string, string>? Param {get; init;}
    public required IReadOnlyDictionary<string, string?>? Query {get; init;}
}
