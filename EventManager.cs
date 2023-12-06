using System.Reflection;

namespace Undefined.Events;

public static class EventManager
{
    private static readonly Dictionary<Type, Event<IEventArgs>> Actions = new();
    private static readonly object LockObj = new();

    public static void RegisterEvent<T>(EventHandler<T> handler, Priority priority = Priority.Normal)
        where T : IEventArgs
    {
        lock (LockObj)
        {
            var type = typeof(T);
            if (!Actions.TryGetValue(type, out var e))
            {
                e = new Event<IEventArgs>();
                Actions.Add(type, e);
            }

            e.AddListener(args => handler((T)args), priority);
        }
    }

    internal static void OnRaise<T>(ref T args) where T : IEventArgs
    {
        var type = typeof(T);
        lock (LockObj)
            foreach (var (key, value) in Actions)
                if (key.IsAssignableFrom(type))
                    value.Raise(args);
    }

    public static void RegisterEvents(IEventsHandler handler) => RegisterEvents(handler.GetType(), handler);
    public static void RegisterStaticEvents<T>() => RegisterEvents(typeof(T), null);

    private static void RegisterEvents(Type listenerType, IEventsHandler? listener)
    {
        var flags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public;
        var isStatic = listener is null;
        if (isStatic)
            flags |= BindingFlags.Static;
        else
            flags |= BindingFlags.Instance;
        var baseType = listenerType;
        lock (LockObj)
        {
            while (baseType != null)
            {
                foreach (var method in baseType.GetMethods(flags))
                {
                    var arguments = method.GetParameters();
                    if (method.GetCustomAttributes().FirstOrDefault(att => att is EventHandlerAttribute) is
                            not EventHandlerAttribute attribute || arguments.Length != 1 ||
                        !typeof(IEventArgs).IsAssignableFrom(arguments[0].ParameterType)) continue;
                    var eventType = arguments[0].ParameterType;

                    if (!Actions.TryGetValue(eventType, out var e))
                    {
                        e = new Event<IEventArgs>();
                        Actions.Add(eventType, e);
                    }

                    var del = method.CreateDelegate(typeof(EventHandler<>).MakeGenericType(eventType));
                    e.Add(new Listener(e, del, attribute.Priority));
                }

                if (isStatic) break;
                baseType = baseType.BaseType;
            }
        }
    }
}