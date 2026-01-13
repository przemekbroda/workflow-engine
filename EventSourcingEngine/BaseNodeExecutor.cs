using EventSourcingEngine.Exceptions;

namespace EventSourcingEngine;

public abstract class BaseNodeExecutor<TState, TEvent> : INodeExecutor<TState, TEvent> 
    where TState : new()
    where TEvent : Event
{
    public required Cursor<TState, TEvent> Cursor { get; set; }
    public required HashSet<string> ProducesEvents { get; set; }
    public required HashSet<string> HandlesEvents { get; set; }
    
    public abstract Task UpdateState(TEvent e, CancellationToken cancellationToken);
    public abstract Task<TEvent> ExecuteAsync(TEvent e, CancellationToken cancellationToken);

    public async Task TryUpdateState(TEvent e, CancellationToken cancellationToken)
    {
        if (!ProducesEvents.Contains(e.EventName))
        {
            throw new EventSourcingEngineException("Cannot handle state update");
        }

        await UpdateState(e, cancellationToken);
    }
}