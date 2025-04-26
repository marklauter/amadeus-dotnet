namespace Amadeus.Nine.Locks;

internal sealed class AsyncLock
    : IDisposable
{
    internal sealed class Lock(AsyncLock owner)
        : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            _ = owner.Release();
        }
    }

    private bool disposed;
    private readonly SemaphoreSlim latch = new(1, 1);

    /// <summary>
    /// Aquires a disposable lock object.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="Lock"/></returns>
    /// <example>
    /// using var releaser = await AquireLock()
    /// </example>
    public async Task<Lock> AquireAsync(CancellationToken cancellationToken)
    {
        await NotDisposed().latch.WaitAsync(cancellationToken);
        return new Lock(this);
    }

    public int Release() => latch.Release();

    public async Task<TReturn> WithLockAsync<TReturn>(Func<CancellationToken, ValueTask<TReturn>> func, CancellationToken cancellationToken)
    {
        await NotDisposed().latch.WaitAsync(cancellationToken);
        try
        {
            return await func(cancellationToken);
        }
        finally
        {
            _ = latch.Release();
        }
    }

    public async Task<TReturn> WithLockAsync<TReturn>(Func<CancellationToken, Task<TReturn>> func, CancellationToken cancellationToken)
    {
        await NotDisposed().latch.WaitAsync(cancellationToken);
        try
        {
            return await func(cancellationToken);
        }
        finally
        {
            _ = latch.Release();
        }
    }

    public async Task<TReturn> WithLockAsync<TReturn>(Func<TReturn> func, CancellationToken cancellationToken)
    {
        await NotDisposed().latch.WaitAsync(cancellationToken);
        try
        {
            return func();
        }
        finally
        {
            _ = latch.Release();
        }
    }

    public async Task WithLockAsync(Action action, CancellationToken cancellationToken)
    {
        await NotDisposed().latch.WaitAsync(cancellationToken);
        try
        {
            action();
        }
        finally
        {
            _ = latch.Release();
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        latch.Dispose();

        disposed = true;
    }

    private AsyncLock NotDisposed() => disposed
        ? throw new ObjectDisposedException(nameof(AsyncLock))
        : this;
}
