namespace EventSourcingEngine.UnitTests.TreeProviderTests.Nodes;

public record TreeEvent(string EventName, object? Payload) : Event(EventName, Payload);