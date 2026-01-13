namespace EventSourcingEngine;

public interface INodeExecutor<TState> where TState : new()
{
    public Cursor<TState> Cursor { get; set; }
    HashSet<string> HandlesEvents { get; set; }
    HashSet<string> ProducesEvents { get; set; }
    Task<Event> ExecuteAsync(Event e, CancellationToken cancellationToken);
    Task TryUpdateState(Event e, CancellationToken cancellationToken);
}