namespace Undefined.Events;


public sealed class Listener
{
    private readonly IEvent _event;
    internal Priority Priority { get; }
    internal Delegate Delegate { get; }
    internal bool RequireListener { get; }
    internal Listener(IEvent @event, Delegate @delegate, Priority priority, bool requireListener)
    {
        _event = @event;
        Delegate = @delegate;
        Priority = priority;
        RequireListener = requireListener;
    }

    public void Detach() => _event.DetachListener(this);
}