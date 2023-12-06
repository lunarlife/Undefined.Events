namespace Undefined.Events;

public delegate void EventHandler();

public delegate void EventHandler<in T>(T args) where T : IEventArgs;

public interface IEvent
{
    public IReadOnlyList<Listener> Listeners { get; }
    public void DetachListener(Listener listener);
}

public class EventBase : IEvent
{
    private readonly Dictionary<Priority, List<Listener>> _eventListenersPriority = new();
    private readonly List<Listener> _eventListeners = new();
    public IReadOnlyList<Listener> Listeners => _eventListeners.AsReadOnly();
    private readonly object _lockObj = new();

    internal EventBase()
    {
    }

    public void DetachListener(Listener listener)
    {
        lock (_lockObj)
            if (!_eventListeners.Remove(listener) ||
                !_eventListenersPriority.TryGetValue(listener.Priority, out var list) || !list.Remove(listener))
                throw new EventException("This listener is not registered for this event.");
    }

    protected void Raise<T>(ref T? value) where T : IEventArgs
    {
        lock (_lockObj)
            for (var priority = Priority.Lowest; priority <= Priority.Monitor; priority++)
            {
                if (!_eventListenersPriority.TryGetValue(priority, out var list))
                    continue;
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    var listener = list[i];
                    ((EventHandler<T>)listener.Delegate).Invoke(value!);
                }
            }
    }

    protected void Raise()
    {
        lock (_lockObj)
            for (var priority = Priority.Lowest; priority <= Priority.Monitor; priority++)
            {
                if (!_eventListenersPriority.TryGetValue(priority, out var list))
                    continue;
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    var listener = list[i];
                    ((EventHandler)listener.Delegate).Invoke();
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

    public new void Raise() => base.Raise();

    public Listener AddListener(EventHandler handler, Priority priority = Priority.Normal) =>
        Add(new Listener(this, handler, priority));
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
        Raise(ref args!);
        EventManager.OnRaise(ref args);
    }

    public Listener AddListener(EventHandler<T> handler, Priority priority = Priority.Normal) =>
        Add(new Listener(this, handler, priority));
}