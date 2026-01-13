namespace EventSourcingEngine;

public record Event(string EventName, object? Payload);