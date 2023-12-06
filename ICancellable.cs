namespace Undefined.Events;

public interface ICancellable
{
    public bool IsCancelled { get; set; }
}