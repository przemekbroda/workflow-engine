namespace EventSourcingEngine;

public class Cursor<TState, TEvent> 
    where TState : class
    where TEvent : class
{
    public TState State { get; set; } = null!;
    public TEvent CurrentEvent => InitEvents.Peek();
    public Stack<TEvent> InitEvents { get; internal init; } = new();
    public Stack<TEvent> ProcessedEvents { get; } = new();
}