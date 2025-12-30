using EventSourcingEngine.Exceptions;

namespace EventSourcingEngine;

public abstract class BaseNodeExecutor<TState, TEvent> : INodeExecutor<TState, TEvent> 
    where TState : new()
    where TEvent : class
{
    public required Cursor<TState, TEvent> Cursor { get; set; }
    public required HashSet<Type> ProducesEvents { get; set; }
    public required HashSet<Type> HandlesEvents { get; set; }

    protected abstract void UpdateState(TEvent e);
    
    public abstract Task<TEvent> ExecuteAsync(TEvent e, CancellationToken cancellationToken);

    public void TryUpdateState(TEvent e)
    {
        if (!ProducesEvents.Contains(e.GetType()))
        {
            throw new EventSourcingEngineException("Cannot handle state update");
        }

        UpdateState(e);
    }
}