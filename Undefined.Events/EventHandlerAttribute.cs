namespace Undefined.Events;

[AttributeUsage(AttributeTargets.Method)]
public sealed class EventHandlerAttribute(Priority priority, bool isOneTime = false) : Attribute
{
    public Priority Priority { get; } = priority;
    public bool IsOneTime { get; } = isOneTime;

    public EventHandlerAttribute(bool isOneTIme = false) : this(Priority.Normal, isOneTIme)
    {
    }
}