namespace Undefined.Events;

[AttributeUsage(AttributeTargets.Method)]
public sealed class EventHandlerAttribute(Priority priority) : Attribute
{
    public EventHandlerAttribute() : this(Priority.Normal)
    {
    }

    public Priority Priority { get; } = priority;
}