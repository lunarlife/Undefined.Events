using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Undefined.Events.Test;

[TestClass]
public class EventTest
{
    private Event _event;
    private bool _eventRaised;

    [TestInitialize]
    public void Setup()
    {
        _event = new Event();
        _eventRaised = false;
    }

    [TestMethod]
    public void Listener_Can_Be_Added()
    {
        _event.AddListener(OnEventRaised);
        Assert.AreEqual(1, _event.Listeners.Count);
    }

    [TestMethod]
    public void Event_Can_Be_Raised()
    {
        _event.AddListener(OnEventRaised);
        _event.Raise();
        Assert.IsTrue(_eventRaised);
    }

    [TestMethod]
    public void Listener_Can_Be_Detached()
    {
        var listener = _event.AddListener(OnEventRaised);
        listener.Detach();
        Assert.AreEqual(0, _event.Listeners.Count);
    }
    [TestMethod]
    public void Listener_Can_Be_Detached_Raised()
    {
        _event.AddListener(OnEventRaisedDetach);
        _event.Raise();
        Assert.AreEqual(0, _event.Listeners.Count);
    }
    private void OnEventRaised() => _eventRaised = true;
    private void OnEventRaisedDetach(Listener listener)
    {
        _eventRaised = true;
        listener.Detach();
    }
}