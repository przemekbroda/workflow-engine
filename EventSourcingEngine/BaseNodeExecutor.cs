using EventSourcingEngine.Exceptions;

namespace EventSourcingEngine;

public abstract class BaseNodeExecutor<TState, TEvent> : INodeExecutor<TState, TEvent> 
    where TState : class
    where TEvent : class
{
    public required Cursor<TState, TEvent> Cursor { get; set; }
    public required HashSet<Type> ProducesEvents { get; set; }
    public required HashSet<Type> HandlesEvents { get; set; }

    protected abstract void UpdateState(TEvent e);
    
    public abstract Task<TEvent> ExecuteAsync(TEvent @event, CancellationToken cancellationToken);

    public void TryUpdateState(TEvent @event)
    {
        if (!ProducesEvents.Contains(@event.GetType()))
        {
            throw new EventSourcingEngineException($"Cannot handle state update for provided event type {@event.GetType().Name}");
        }

        UpdateState(@event);
    }

    public virtual Task AfterExecutionAndStateUpdate(TEvent @event, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}