namespace Undefined.Events;

public interface IEventAccess
{
    public Listener AddListener(EventHandler handler, Priority priority = Priority.Normal);
    public Listener AddListener(EventHandlerListener handler, Priority priority = Priority.Normal);

}

public interface IEventAccess<out T> where T : IEventArgs
{
    public Listener AddListener(EventHandler<T> handler, Priority priority = Priority.Normal);
    public Listener AddListener(EventHandlerListener<T> handler, Priority priority = Priority.Normal);

}

internal sealed class EventAccess : IEventAccess
{
    private readonly Event _event;

    internal EventAccess(Event @event) => _event = @event;

    public Listener AddListener(EventHandler handler, Priority priority = Priority.Normal) => _event.AddListener(handler, priority);
    public Listener AddListener(EventHandlerListener handler, Priority priority = Priority.Normal) => _event.AddListener(handler, priority);

}

internal sealed class EventAccess<T> : IEventAccess<T> where T : IEventArgs
{
    private readonly Event<T> _event;

    internal EventAccess(Event<T> @event) => _event = @event;

    public Listener AddListener(EventHandler<T> handler, Priority priority = Priority.Normal) => _event.AddListener(handler, priority);
    public Listener AddListener(EventHandlerListener<T> handler, Priority priority = Priority.Normal) => _event.AddListener(handler, priority);
}