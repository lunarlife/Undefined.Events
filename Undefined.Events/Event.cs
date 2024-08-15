namespace Undefined.Events;

public delegate void EventHandler();

public delegate void EventHandler<in T>(T args) where T : IEventArgs;

public delegate void EventHandlerListener(Listener listener);

public delegate void EventHandlerListener<in T>(T args, Listener listener) where T : IEventArgs;

public interface IEvent : IDisposable
{
    public IReadOnlyList<Listener> Listeners { get; }
    public bool IsDisposed { get; }
    public void DetachListener(Listener listener);
    public void DetachAllListeners();
}

public class EventBase : IEvent
{
    private readonly List<Listener> _eventListeners = [];
    private readonly Dictionary<Priority, List<Listener>> _eventListenersPriority = new();
    private readonly object _lockObj = new();
    private bool _isDisposed;

    public IReadOnlyList<Listener> Listeners => _eventListeners.AsReadOnly();

    public bool IsDisposed => _isDisposed;

    internal EventBase()
    {
    }

    public void DetachAllListeners()
    {
        CheckIsDisposed();
        lock (_lockObj)
        {
            for (var i = _eventListeners.Count - 1; i >= 0; i--)
            {
                var listener = _eventListeners[i];
                listener.Detach();
            }
        }
    }

    public void DetachListener(Listener listener)
    {
        CheckIsDisposed();
        lock (_lockObj)
            if (!_eventListeners.Remove(listener) ||
                !_eventListenersPriority.TryGetValue(listener.Priority, out var list) || !list.Remove(listener))
                throw new EventException("This listener is not registered for this event.");
    }

    protected void Raise<T>(T? value) where T : IEventArgs
    {
        CheckIsDisposed();
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
        CheckIsDisposed();
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

    public virtual void Dispose()
    {
        CheckIsDisposed();
        DetachAllListeners();
        _isDisposed = true;
    }

    private void CheckIsDisposed()
    {
        if (_isDisposed) throw new ObjectDisposedException(null);
    }
}

public sealed class Event : EventBase
{
    public IEventAccess Access { get; }

    public Event()
    {
        Access = new EventAccess(this);
    }

    public override void Dispose()
    {
        base.Dispose();
        Access.Dispose();
    }

    public void Raise() => base.Raise<IEventArgs>(null);
    public async Task RaiseAsync() => await Task.Run(Raise);

    public Listener AddListener(EventHandler handler, Priority priority = Priority.Normal) =>
        Add(new Listener(this, handler, priority, false));

    public Listener AddListener(EventHandlerListener handler, Priority priority = Priority.Normal) =>
        Add(new Listener(this, handler, priority, true));
}

public sealed class Event<T> : EventBase where T : IEventArgs
{
    public IEventAccess<T> Access { get; }
    private readonly bool _isStatic;

    public Event()
    {
        Access = new EventAccess<T>(this);
    }

    internal Event(bool isStatic)
    {
        _isStatic = isStatic;
    }

    public RaiseResult<T> Raise(T args)
    {
        base.Raise(args);
        if (!_isStatic)
            EventsManager.OnRaiseInternal(args);
        return new RaiseResult<T>(args);
    }

    public async Task<RaiseResult<T>> RaiseAsync(T args)
    {
        return await Task.Run(() => Raise(args));
    }

    public override void Dispose()
    {
        base.Dispose();
        Access.Dispose();
    }

    public Listener AddListener(EventHandler<T> handler, Priority priority = Priority.Normal) =>
        Add(new Listener(this, handler, priority, false));

    public Listener AddListener(EventHandlerListener<T> handler, Priority priority = Priority.Normal) =>
        Add(new Listener(this, handler, priority, true));
}