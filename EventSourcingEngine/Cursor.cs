namespace EventSourcingEngine;

public class Cursor<TState> where TState : new()
{
    public required TState State { get; set; }
    public Event CurrentEvent => InitEvents.Peek();
    public Stack<Event> InitEvents { get; init; } = new();
    public Stack<Event> ProcessedEvents { get; } = new();
}