using System.Diagnostics.CodeAnalysis;

namespace BlazorQuery.Core;

public enum QueryStatus
{
    Uninitialized = 0,
    Loading = 1,
    Succeeded = 2,
    Failed = 3,
}

public sealed class QueryStatusChangedEventArgs : EventArgs
{
    public required QueryStatus Status { get; init; }
}

public sealed class QuerySucceededEventArgs<TArgs, TResult> : EventArgs
    where TArgs : IEquatable<TArgs>
{
    public required TArgs Args { get; init; }

    public required TResult Result { get; init; }
}

public sealed class QueryFailedEventArgs<TArgs> : EventArgs
    where TArgs : IEquatable<TArgs>
{
    public required TArgs Args { get; init; }

    public required Exception Error { get; init; }
}

public interface IQuery<TArgs, TResult> : IDisposable
    where TArgs : IEquatable<TArgs>
{
    public QueryStatus Status { get; }

    public TArgs? Args { get; set; }

    public TResult? Result { get; }

    public Exception? Error { get; }

    public event Action<QueryStatusChangedEventArgs> OnStatusChanged;

    public event Action<QuerySucceededEventArgs<TArgs, TResult>> OnSucceeded;

    public event Action<QueryFailedEventArgs<TArgs>> OnFailed;

    /*
        public void Detach();

        public void Cancel();

        public void Trigger();
    */

    public ValueTask TriggerAsync(CancellationToken cancellationToken = default);
}

public sealed class QueryOptions
{
    public TimeSpan ReloadingDuration { get; init; } = TimeSpan.FromMinutes(5);

    public TimeSpan StaleDuration { get; init; } = TimeSpan.Zero;

    public IRetryHandler RetryHandler { get; init; } = NoRetryHandler.Instance;
}

internal sealed class Query<TArgs, TResult>(
    Func<TArgs, CancellationToken, ValueTask<TResult>> queryFunc,
    QueryOptions queryOptions) : IQuery<TArgs, TResult>
    where TArgs : IEquatable<TArgs>
{
    private readonly Func<TArgs, CancellationToken, ValueTask<TResult>> _queryFunc = queryFunc;
    private readonly QueryOptions _queryOptions = queryOptions;

    private readonly QueryCache<TArgs, TResult> _queryCache = new(queryFunc, queryOptions);
    private QueryStatus _status = QueryStatus.Uninitialized;

    public QueryStatus Status
    {
        get => _status;
        set
        {
            _status = value;

            OnStatusChanged?.Invoke(new() { Status = _status });
        }
    }

    public TArgs? Args { get; set; } = default;

    public TResult? Result { get; private set; } = default;

    public Exception? Error { get; private set; } = default;

    public event Action<QueryStatusChangedEventArgs>? OnStatusChanged;

    public event Action<QuerySucceededEventArgs<TArgs, TResult>>? OnSucceeded;

    public event Action<QueryFailedEventArgs<TArgs>>? OnFailed;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public async ValueTask TriggerAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(cancellationToken);
    }

    private async ValueTask ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Status == QueryStatus.Uninitialized || Args is null)
        {
            throw new InvalidOperationException("No Query.Args were set. Query is NOT initialized.");
        }

        Status = QueryStatus.Loading;

        try
        {
            Result = await _queryCache.ExecuteAsync(Args, cancellationToken);

            Status = QueryStatus.Succeeded;
            OnSucceeded?.Invoke(new() { Args = Args, Result = Result });
        }
        catch (OperationCanceledException)
        {
            // No operation
        }
        catch (Exception error)
        {
            Error = error;

            Status = QueryStatus.Failed;
            OnFailed?.Invoke(new() { Args = Args, Error = error });
        }
    }
}
