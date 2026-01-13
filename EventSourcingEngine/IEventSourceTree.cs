namespace EventSourcingEngine;

public interface IEventSourceTree<TState, TEvent, TTreeProvider>
    where TState : new()
    where TEvent : class
    where TTreeProvider : TreeProvider<TState, TEvent>
{
    
    /// <summary>
    /// Executes event sourcing tree with given state initializer
    /// </summary>
    /// <param name="initialCursorEvents"></param>
    /// <param name="stateInitializer"></param>
    /// <param name="cancellationToken"></param>
    public Task<ExecuteTreeResult<TState, TEvent>> ExecuteTree(IEnumerable<TEvent> initialCursorEvents, Func<TEvent, TState> stateInitializer, CancellationToken cancellationToken);
}