namespace Undefined.Events;

public sealed class Listener : IDisposable
{
    private readonly IEvent _event;
    
    internal Priority Priority { get; }
    internal Delegate Delegate { get; }
    internal bool RequireListener { get; }
    
    public bool IsAttached { get; private set; } = true;

    internal Listener(IEvent @event, Delegate @delegate, Priority priority, bool requireListener)
    {
        _event = @event;
        Delegate = @delegate;
        Priority = priority;
        RequireListener = requireListener;
    }

    public void Detach()
    {
        CheckIsAttached();
        _event.DetachListener(this);
        IsAttached = false;
    }

    private void CheckIsAttached()
    {
        if (!IsAttached)
            throw new EventException("Listener is no longer attached.");
    }

    public void Dispose()
    {
        CheckIsAttached();
        Detach();
    }
}