namespace Undefined.Events;


public sealed class Listener
{
    private readonly IEvent _event;
    public Priority Priority { get; }

    public Delegate Delegate { get; }

    internal Listener(IEvent @event, Delegate @delegate, Priority priority)
    {
        _event = @event;
        Delegate = @delegate;
        Priority = priority;
    }

    public void Detach() => _event.DetachListener(this);
}