namespace EventSourcingEngine;

public record TreeExecutionResult<TState, TEvent>(TState State, TEvent Event) 
    where TState : new() 
    where TEvent : class;