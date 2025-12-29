namespace EventSourcingEngine;

public record EventNode<TState, TEvent>
    where TState : new()
    where TEvent : Event
{
    public required HashSet<string> HandlesEvents { get; init; }
    public required Type Executor { get; init; }
    public required HashSet<string> ProducesEvents { get; init; }
    public required List<EventNode<TState, TEvent>> NextExecutors { get; init; }
}

internal record EventNodeInst<TState, TEvent>(INodeExecutor<TState, TEvent> Executor, List<EventNodeInst<TState, TEvent>> NextExecutors) 
    where TState : new()
    where TEvent : Event;