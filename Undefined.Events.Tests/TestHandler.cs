namespace Undefined.Events.Tests;

public class TestHandler : IEventsHandler
{
    public bool IsRaised { get; set; }

    [EventHandler]
    private void OnTest(TestEventArgs args)
    {
        if (args.Value != Tests.TEST_ARGS_VALUE) Assert.Fail();
        IsRaised = true;
    }


    private void OnTestWithoutAttribute(TestEventArgs args)
    {
        if (args.Value != Tests.TEST_ARGS_VALUE) Assert.Fail();
        IsRaised = true;
    }
}