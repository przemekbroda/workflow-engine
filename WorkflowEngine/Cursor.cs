namespace EventSourcingEngine;

public class Cursor<TState, TEvent> 
    where TState : class
    where TEvent : class
{
    internal Stack<TEvent> InitEventsStack { get; init; } = new();
    internal Stack<TEvent> ProcessedEventsStack { get; } = new();
    
    public TState State { get; internal set; } = null!;
    public TEvent CurrentEvent => InitEventsStack.Peek();
    public IReadOnlyList<TEvent> ProcessedEvents => ProcessedEventsStack.ToList();
    public IReadOnlyList<TEvent> InitEvents => InitEventsStack.ToList();
}