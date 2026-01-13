using EventSourcingEngine.Exceptions;

namespace EventSourcingEngine;

public abstract class BaseNodeExecutor<TState> : INodeExecutor<TState> where TState : new()
{
    public Cursor<TState> Cursor { get; set; }
    public HashSet<string> ProducesEvents { get; set; }
    public HashSet<string> HandlesEvents { get; set; }
    public abstract Task UpdateState(Event e, CancellationToken cancellationToken);
    public abstract Task<Event> ExecuteAsync(Event e, CancellationToken cancellationToken);

    public async Task TryUpdateState(Event e, CancellationToken cancellationToken)
    {
        if (!ProducesEvents.Contains(e.EventName))
        {
            throw new EventSourcingEngineException("Cannot handle state update");
        }

        await UpdateState(e, cancellationToken);
    }
}