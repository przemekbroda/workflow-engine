namespace EventSourcingEngine;

public record EventNode<TState, TEvent>(HashSet<string> HandlesEvents, Type Executor, HashSet<string> ProducesEvents, List<EventNode<TState, TEvent>> NextExecutors)
    where TState : new()
    where TEvent : Event;

internal record EventNodeInst<TState, TEvent>(INodeExecutor<TState, TEvent> Executor, List<EventNodeInst<TState, TEvent>> NextExecutors) 
    where TState : new()
    where TEvent : Event;