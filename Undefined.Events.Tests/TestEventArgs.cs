namespace Undefined.Events.Tests;

public class TestEventArgs : IEventArgs
{
    public int Value { get; }

    public TestEventArgs(int value)
    {
        Value = value;
    }   
}