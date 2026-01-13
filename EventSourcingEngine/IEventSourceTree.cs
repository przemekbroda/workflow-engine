namespace EventSourcingEngine;

public interface IEventSourceTree<TState, TEvent, TTreeProvider>
    where TState : class
    where TEvent : class
    where TTreeProvider : TreeProvider<TState, TEvent>
{
    IReadOnlyList<Type> HandlesEvents { get; }
    
    /// <summary>
    /// Executes event sourcing tree with given state initializer
    /// </summary>
    /// <param name="events"></param>
    /// <param name="stateInitializer">State initializer</param>
    /// <param name="cancellationToken"></param>
    public Task<ExecuteTreeResult<TState, TEvent>> ExecuteTree(IList<TEvent> events, Func<TEvent, TState> stateInitializer, CancellationToken cancellationToken);

    /// <summary>
    /// Used solely to recreate state based on provided events and state initializer
    /// </summary>
    /// <param name="events">Events for state recreation</param>
    /// <param name="stateInitializer">State initializer</param>
    /// <returns>Recreated state</returns>
    public TState RecreateState(IList<TEvent> events, Func<TEvent, TState> stateInitializer);
}