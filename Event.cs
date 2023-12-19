namespace Undefined.Events;

public delegate void EventHandler();

public delegate void EventHandler<in T>(T args) where T : IEventArgs;

public delegate void EventHandlerListener(Listener listener);

public delegate void EventHandlerListener<in T>(T args, Listener listener) where T : IEventArgs;

public interface IEvent
{
    public IReadOnlyList<Listener> Listeners { get; }
    public void DetachListener(Listener listener);
    public void DetachAllListeners();
    
}

public class EventBase : IEvent
{
    private readonly Dictionary<Priority, List<Listener>> _eventListenersPriority = new();
    private readonly List<Listener> _eventListeners = [];
    public IReadOnlyList<Listener> Listeners => _eventListeners.AsReadOnly();
    private readonly object _lockObj = new();

    internal EventBase()
    {
    }

    public void DetachAllListeners()
    {
        lock (_lockObj)
        {
            for (var i = 0; i < _eventListeners.Count; i++) _eventListeners.RemoveAt(i);
            _eventListenersPriority.Clear();
        }
    }

    public void DetachListener(Listener listener)
    {
        lock (_lockObj)
            if (!_eventListeners.Remove(listener) ||
                !_eventListenersPriority.TryGetValue(listener.Priority, out var list) || !list.Remove(listener))
                throw new EventException("This listener is not registered for this event.");
    }

    protected void Raise<T>(T? value) where T : IEventArgs
    {
        lock (_lockObj)
            for (var priority = Priority.Lowest; priority <= Priority.Monitor; priority++)
            {
                if (!_eventListenersPriority.TryGetValue(priority, out var list))
                    continue;
                if (value is null)
                    for (var i = list.Count - 1; i >= 0; i--)
                    {
                        var listener = list[i];
                        if (listener.RequireListener)
                            ((EventHandlerListener)listener.Delegate).Invoke(listener);
                        else ((EventHandler)listener.Delegate).Invoke();
                    }
                else
                    for (var i = list.Count - 1; i >= 0; i--)
                    {
                        var listener = list[i];
                        if (listener.RequireListener)
                            ((EventHandlerListener<T>)listener.Delegate).Invoke(value, listener);
                        else ((EventHandler<T>)listener.Delegate).Invoke(value);
                    }
            }
    }

    internal Listener Add(Listener listener)
    {
        lock (_lockObj)
        {
            _eventListeners.Add(listener);
            if (!_eventListenersPriority.TryGetValue(listener.Priority, out var list))
            {
                list = new List<Listener>();
                _eventListenersPriority.Add(listener.Priority, list);
            }

            list.Add(listener);
        }

        return listener;
    }
}

public sealed class Event : EventBase
{
    public IEventAccess Access { get; }

    public Event()
    {
        Access = new EventAccess(this);
    }

    public void Raise() => base.Raise<IEventArgs>(null);

    public Listener AddListener(EventHandler handler, Priority priority = Priority.Normal) =>
        Add(new Listener(this, handler, priority, false));

    public Listener AddListener(EventHandlerListener handler, Priority priority = Priority.Normal) =>
        Add(new Listener(this, handler, priority, true));
}

public sealed class Event<T> : EventBase where T : IEventArgs
{
    public IEventAccess<T> Access { get; }

    public Event()
    {
        Access = new EventAccess<T>(this);
    }

    public void Raise(T args)
    {
        base.Raise(args);
        EventManager.OnRaise(args);
    }

    public Listener AddListener(EventHandler<T> handler, Priority priority = Priority.Normal) =>
        Add(new Listener(this, handler, priority, false));

    public Listener AddListener(EventHandlerListener<T> handler, Priority priority = Priority.Normal) =>
        Add(new Listener(this, handler, priority, true));
}