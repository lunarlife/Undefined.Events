namespace Undefined.Events;

[AttributeUsage(AttributeTargets.Method)]
public sealed class EventHandlerAttribute(Priority priority) : Attribute
{
    public Priority Priority { get; } = priority;

    public EventHandlerAttribute() : this(Priority.Normal)
    {
    }
}