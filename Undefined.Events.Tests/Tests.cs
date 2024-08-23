namespace Undefined.Events.Tests;

public class Tests
{
    public const int TEST_ARGS_VALUE = 5;
    private Event<TestEventArgs> _event;
    private IEventAccess<TestEventArgs> _eventAccess;
    private bool _isEventRaised;

    [SetUp]
    public void Setup()
    {
        _event = new Event<TestEventArgs>();
        _eventAccess = _event.Access;
    }

    [TearDown]
    public void TearDown()
    {
        _event.Dispose();
    }
    [Test]
    public void RaiseEvent()
    {
        var listener = _eventAccess.AddListener(OnEventRaised);
        _event.Raise(new TestEventArgs(TEST_ARGS_VALUE));
        if (!_isEventRaised) Assert.Fail();
        listener.Detach();
        _isEventRaised = false;

        _event.Raise(new TestEventArgs(TEST_ARGS_VALUE));
        if (_isEventRaised) Assert.Fail();
        Assert.Pass();
    }
    [Test]
    public void RaiseOneTimeEvent()
    {
        var listener = _eventAccess.AddOneTimeListener(OnEventRaised);
        _event.Raise(new TestEventArgs(TEST_ARGS_VALUE));
        if (!_isEventRaised) Assert.Fail();
        Assert.Catch(listener.Detach);
        _isEventRaised = false;

        _event.Raise(new TestEventArgs(TEST_ARGS_VALUE));
        if (_isEventRaised) Assert.Fail();
        Assert.Pass();
    }

    [Test]
    public void RaiseStaticEvent()
    {
        var listener = EventsManager.AddStaticListener<TestEventArgs>(OnEventRaised);
        _event.Raise(new TestEventArgs(TEST_ARGS_VALUE));
        if (!_isEventRaised) Assert.Fail();
        listener.Detach();
        _isEventRaised = false;

        _event.Raise(new TestEventArgs(TEST_ARGS_VALUE));
        if (_isEventRaised) Assert.Fail();
        Assert.Pass();
    }

    [Test]
    public void RaiseStaticHandlerEvent()
    {
        var handler = new TestHandler();
        var listeners = EventsManager.AddStaticListeners(handler);
        if (listeners.Count != 1) Assert.Fail();
        _event.Raise(new TestEventArgs(TEST_ARGS_VALUE));
        if (!handler.IsRaised) Assert.Fail();
        foreach (var listener in listeners) listener.Detach();
        handler.IsRaised = false;
        
        _event.Raise(new TestEventArgs(TEST_ARGS_VALUE));
        if (handler.IsRaised) Assert.Fail();
        Assert.Pass();
    }

    private void OnEventRaised(TestEventArgs args)
    {
        if (args.Value != TEST_ARGS_VALUE) Assert.Fail();
        _isEventRaised = true;
    }
}