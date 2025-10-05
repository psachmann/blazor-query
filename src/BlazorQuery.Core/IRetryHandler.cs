namespace BlazorQuery.Core;

public interface IRetryHandler
{
    public ValueTask<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, ValueTask<TResult>> func,
        CancellationToken cancellationToken
    );
}

internal sealed class NoRetryHandler : IRetryHandler
{
    public static readonly IRetryHandler Instance = new NoRetryHandler();

    public ValueTask<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, ValueTask<TResult>> func,
        CancellationToken cancellationToken
    )
    {
        return func(cancellationToken);
    }
}
