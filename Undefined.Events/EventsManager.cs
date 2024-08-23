using System.Reflection;

namespace Undefined.Events;

public static class EventsManager
{
    private static readonly Dictionary<Type, EventBase> Actions = new();
    private static readonly object LockObj = new();

    public static Listener AddStaticListener<T>(EventHandler<T> handler, Priority priority = Priority.Normal)
        where T : IEventArgs
    {
        lock (LockObj)
            return GetEvent<T>().AddListener(handler, priority);
    }

    public static Listener AddOneTimeStaticListener<T>(EventHandler<T> handler, Priority priority = Priority.Normal)
        where T : IEventArgs
    {
        lock (LockObj)
            return GetEvent<T>().AddOneTimeListener(handler, priority);
    }

    public static Listener AddStaticListener<T>(EventHandlerListener<T> handler, Priority priority = Priority.Normal)
        where T : IEventArgs
    {
        lock (LockObj)
            return GetEvent<T>().AddListener(handler, priority);
    }

    public static Listener AddOneTimeStaticListener<T>(EventHandlerListener<T> handler,
        Priority priority = Priority.Normal)
        where T : IEventArgs
    {
        lock (LockObj)
            return GetEvent<T>().AddOneTimeListener(handler, priority);
    }

    private static Event<T> GetEvent<T>() where T : IEventArgs
    {
        var type = typeof(T);
        if (!Actions.TryGetValue(type, out var e))
        {
            e = new Event<T>(true);
            Actions.Add(type, e);
        }

        return (Event<T>)e;
    }

    internal static void OnRaiseInternal<T>(T args) where T : IEventArgs
    {
        var type = typeof(T);
        lock (LockObj)
            foreach (var pair in Actions)
                if (pair.Key.IsAssignableFrom(type))
                    ((Event<T>)pair.Value).Raise(args);
    }

    public static IReadOnlyList<Listener> AddStaticListeners(IEventsHandler handler) =>
        AddStaticListeners(handler.GetType(), handler);


    public static IReadOnlyList<Listener> AddStaticListeners<T>() => AddStaticListeners(typeof(T), null);

    private static IReadOnlyList<Listener> AddStaticListeners(Type listenerType, IEventsHandler? listener)
    {
        var listeners = new List<Listener>();
        var flags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public;
        var isStatic = listener is null;
        if (isStatic)
            flags |= BindingFlags.Static;
        else
            flags |= BindingFlags.Instance;
        var baseType = listenerType;
        lock (LockObj)
            while (baseType != null)
            {
                foreach (var method in baseType.GetMethods(flags))
                {
                    var arguments = method.GetParameters();
                    if (method.GetCustomAttributes().FirstOrDefault(att => att is EventHandlerAttribute) is
                            not EventHandlerAttribute attribute || arguments.Length < 1 || arguments.Length > 2 ||
                        !typeof(IEventArgs).IsAssignableFrom(arguments[0].ParameterType)) continue;
                    var eventType = arguments[0].ParameterType;

                    if (!Actions.TryGetValue(eventType, out var e))
                    {
                        e = new Event<IEventArgs>();
                        Actions.Add(eventType, e);
                    }

                    var hasListener = arguments.Length == 2;

                    var delegateType = (hasListener ? typeof(EventHandlerListener<>) : typeof(EventHandler<>))
                        .MakeGenericType(eventType);
                    var del = isStatic
                        ? method.CreateDelegate(delegateType)
                        : method.CreateDelegate(delegateType, listener);
                    var l = new Listener(e, del, attribute.Priority, hasListener, attribute.IsOneTime);
                    listeners.Add(l);
                    e.Add(l);
                }

                if (isStatic) break;
                baseType = baseType.BaseType;
            }

        return listeners;
    }
}