namespace EventSourcingEngine;

public record ExecuteTreeResult<TState, TEvent>(TState ProducedState, TEvent ProducedEvent);