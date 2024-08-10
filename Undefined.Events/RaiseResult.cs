namespace Undefined.Events;

public struct RaiseResult<T> where T : IEventArgs
{
    public T Args { get; }
    public bool IsCancelled { get; }

    public RaiseResult(T args)
    {
        Args = args;
        IsCancelled = Args is ICancellable { IsCancelled: true };
    }
}