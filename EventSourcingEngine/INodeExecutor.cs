namespace EventSourcingEngine;

public interface INodeExecutor<TState, TEvent> 
    where TState : class
    where TEvent : class
{
    public Cursor<TState, TEvent> Cursor { get; set; }
    HashSet<Type> HandlesEvents { get; set; }
    HashSet<Type> ProducesEvents { get; set; }
    Task<TEvent> ExecuteAsync(TEvent @event, CancellationToken cancellationToken);
    void TryUpdateState(TEvent @event);
    Task AfterExecutionAndStateUpdate(TEvent @event, CancellationToken cancellationToken);
}