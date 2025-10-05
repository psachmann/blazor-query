namespace BlazorQuery.Core;

public sealed class EndpointOptions { }

public sealed class Endpoint<TParams, TResult>
    where TParams : IEquatable<TParams> { }
