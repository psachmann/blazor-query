namespace BlazorQuery.Core;

internal class QueryCache<TArgs, TResult>(
    Func<TArgs, CancellationToken, ValueTask<TResult>> queryFunc,
    QueryOptions queryOptions
)
    where TArgs : IEquatable<TArgs>
{
    private readonly Func<TArgs, CancellationToken, ValueTask<TResult>> _queryFunc = queryFunc;
    private readonly QueryOptions _queryOptions = queryOptions;
    private readonly Dictionary<TArgs, TResult> _cache = [];

    public async ValueTask<TResult> ExecuteAsync(TArgs args, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(args, out TResult? cachedResult))
        {
            return cachedResult;
        }

        var result = await _queryOptions.RetryHandler.ExecuteAsync(
            (ct) => _queryFunc(args, ct),
            cancellationToken
        );

        _cache[args] = result;

        return result;
    }

    public void Invalidate(TArgs args)
    {
        _cache.Remove(args);
    }

    public void InvalidateAll()
    {
        _cache.Clear();
    }
}
