namespace EventSourcingEngine;

public interface INodeExecutor<TState, TEvent> 
    where TState : new()
    where TEvent : Event
{
    public Cursor<TState, TEvent> Cursor { get; set; }
    HashSet<string> HandlesEvents { get; set; }
    HashSet<string> ProducesEvents { get; set; }
    Task<TEvent> ExecuteAsync(TEvent e, CancellationToken cancellationToken);
    Task TryUpdateState(TEvent e, CancellationToken cancellationToken);
}