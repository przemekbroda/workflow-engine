using EventSourcingEngine.Exceptions;

namespace EventSourcingEngine;

public abstract class BaseNodeExecutor<TState, TEvent> : INodeExecutor<TState, TEvent> 
    where TState : struct
    where TEvent : class
{
    public required Cursor<TState, TEvent> Cursor { get; set; }
    public required HashSet<Type> ProducesEvents { get; set; }
    public required HashSet<Type> HandlesEvents { get; set; }

    protected abstract TState UpdateState(TEvent e, TState state);
    
    public abstract Task<TEvent> ExecuteAsync(TEvent @event, TState state, CancellationToken cancellationToken);

    public TState TryUpdateState(TEvent @event, TState state)
    {
        if (!ProducesEvents.Contains(@event.GetType()))
        {
            throw new EventSourcingEngineException($"Cannot handle state update for provided event type {@event.GetType().Name}");
        }

        return UpdateState(@event, state);
    }

    public virtual Task AfterExecutionAndStateUpdate(TEvent @event, TState state, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}