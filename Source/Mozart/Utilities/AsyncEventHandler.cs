namespace Mozart;

public delegate Task AsyncEventHandler<in TEventArgs>(object? sender, TEventArgs e);

public static class AsyncEventHandlerExtensions
{
    public static Task InvokeAsync<T>(this AsyncEventHandler<T>? handler, object? sender, T e)
    {
        var invocables = handler?.GetInvocationList().Cast<AsyncEventHandler<T>>();
        if (invocables == null)
            return Task.CompletedTask;

        return Task.WhenAll(invocables.Select(h => h.Invoke(sender, e)));
    }
}
