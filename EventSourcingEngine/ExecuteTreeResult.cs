namespace EventSourcingEngine;

public record ExecuteTreeResult<TState, TEvent>(TState ProducedState, TEvent ProducedEvent) 
    where TState : struct 
    where TEvent : class;