namespace EventSourcingEngine;

public interface IEventSourceTree<TState, TEvent>
    where TState : new()
    where TEvent : Event
{
    
    /// <summary>
    /// Executes event sourcing tree with given state initializer
    /// </summary>
    /// <param name="initialCursorEvents"></param>
    /// <param name="stateInitializer"></param>
    /// <param name="cancellationToken"></param>
    public Task<Event> ExecuteTree(IEnumerable<TEvent> initialCursorEvents, Func<object?, TState> stateInitializer, CancellationToken cancellationToken);
}