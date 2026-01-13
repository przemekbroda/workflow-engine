namespace EventSourcingEngine;

public class Cursor<TState, TEvent> 
    where TState : new()
    where TEvent : class
{
    public required TState State { get; set; }
    public TEvent CurrentEvent => InitEvents.Peek();
    public Stack<TEvent> InitEvents { get; internal init; } = new();
    public Stack<TEvent> ProcessedEvents { get; } = new();
}