namespace EventSourcingEngine;

public class Cursor<TState> where TState : new()
{
    public TState State { get; set; }
    public Event CurrentEvent => InitEvents.Peek();
    public Stack<Event> InitEvents { get; set; } = new();
    public Stack<Event> ProcessedEvents { get; set; } = new();
}