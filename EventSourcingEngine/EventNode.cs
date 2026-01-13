namespace EventSourcingEngine;

public record EventNode<TState, TEvent>
    where TState : class
    where TEvent : class
{
    public required HashSet<Type> HandlesEvents { get; init; }
    public required Type Executor { get; init; }
    public required HashSet<Type> ProducesEvents { get; init; }
    public required List<EventNode<TState, TEvent>> NextExecutors { get; init; }
}

internal record EventNodeInst<TState, TEvent>(INodeExecutor<TState, TEvent> Executor, List<EventNodeInst<TState, TEvent>> NextExecutors) 
    where TState : class
    where TEvent : class;