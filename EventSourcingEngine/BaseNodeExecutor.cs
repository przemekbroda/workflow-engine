using EventSourcingEngine.Exceptions;

namespace EventSourcingEngine;

public abstract class BaseNodeExecutor<TState, TEvent> : INodeExecutor<TState, TEvent> 
    where TState : new()
    where TEvent : Event
{
    public required Cursor<TState, TEvent> Cursor { get; set; }
    public required HashSet<string> ProducesEvents { get; set; }
    public required HashSet<string> HandlesEvents { get; set; }

    protected abstract void UpdateState(TEvent e);
    
    public abstract Task<TEvent> ExecuteAsync(TEvent e, CancellationToken cancellationToken);

    public void TryUpdateState(TEvent e)
    {
        if (!ProducesEvents.Contains(e.EventName))
        {
            throw new EventSourcingEngineException("Cannot handle state update");
        }

        UpdateState(e);
    }
}