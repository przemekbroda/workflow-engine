using EventSourcingEngine.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventSourcingEngine;

internal class EventSourceTree<TState, TEvent, TTreeProvider> : IEventSourceTree<TState, TEvent, TTreeProvider>
    where TState : class
    where TEvent : class
    where TTreeProvider : TreeProvider<TState, TEvent>
{
    private readonly EventNode<TState, TEvent> _eventNode;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventSourceTree<TState, TEvent, TTreeProvider>> _logger;
    private readonly TreeProvider<TState, TEvent> _treeProvider;
    
    private EventNodeInst<TState, TEvent> _eventNodeInst = null!;

    public EventSourceTree(
        IServiceProvider serviceProvider, 
        TTreeProvider treeProvider,
        ILogger<EventSourceTree<TState, TEvent, TTreeProvider>> logger)
    {
        _serviceProvider = serviceProvider;
        _treeProvider = treeProvider;
        _logger = logger;
        _eventNode = treeProvider.ProvideTree();
        ResolveTree();
    }
    
    public IReadOnlyList<Type> HandlesEvents => _treeProvider.HandledEvents.ToList();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="events"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="stateInitializer">
    ///     Function that initializes the state, argument of the func is the payload of the first event.
    ///     It is being executed only once just after the ExecuteTree method call. If the first event's payload is null,
    ///     then a passed object to the function will also be null
    /// </param>
    public async Task<ExecuteTreeResult<TState, TEvent>> ExecuteTree(IList<TEvent> events, Func<TEvent, TState> stateInitializer, CancellationToken cancellationToken)
    {
        ValidateInitialCursorEvents(events);

        var cursor = SetupCursor(events, stateInitializer);
        
        //if the tree only contains one init event - initial event, don't pop it from the stack
        if (cursor.InitEvents.Count > 1)
        {
            PopProcessedEvent(cursor);
        }

        var finishedWithEvent = await Resume(cursor, cancellationToken);
        
        _logger.LogDebug("Finished executing event sourcing tree");

        return finishedWithEvent;
    }

    public TState RecreateState(IList<TEvent> events, Func<TEvent, TState> stateInitializer)
    {
        ValidateInitialCursorEvents(events);

        var cursor = SetupCursor(events, stateInitializer);
        
        //if the tree only contains one init event - initial event, don't pop it from the stack
        if (cursor.InitEvents.Count > 1)
        {
            PopProcessedEvent(cursor);
        }

        FindNodeToExecuteAndRecreateState(cursor);

        return cursor.State;
    }

    private void ValidateInitialCursorEvents(IList<TEvent> initialCursorEvents)
    {
        if (initialCursorEvents.Count == 0)
        {
            throw new EventSourcingEngineException("Cannot execute event sourcing tree with empty initial cursor events");
        }

        foreach (var initialCursorEventType in initialCursorEvents.Select(e => e.GetType()))
        {
            if (!HandlesEvents.Contains(initialCursorEventType))
            {
                throw new EventSourcingEngineException($"No node can handle event of type {initialCursorEventType.Name}");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="EventSourcingEngineException"></exception>
    private void ResolveTree()
    {
        if (_eventNode is null)
        {
            throw new EventSourcingEngineException("No nodes were provided for event sourcing engine");
        }
        
        _eventNodeInst = InstantiateNode(_eventNode);
    }

    private Cursor<TState, TEvent> SetupCursor(IEnumerable<TEvent> existingEvents, Func<TEvent, TState> stateInitializer)
    {
        var treeCursor = new Cursor<TState, TEvent>
        {
            InitEvents = new Stack<TEvent>(existingEvents)
        };

        if (!_eventNodeInst.Executor.HandlesEvents.Contains(treeCursor.CurrentEvent.GetType()))
        {
            throw new EventSourceEngineResumeException($"First node is not accepting initial event of this type {treeCursor.CurrentEvent.GetType().Name}");
        }

        treeCursor.State = stateInitializer(treeCursor.CurrentEvent);

        return treeCursor;
    }
    
    private EventNodeInst<TState, TEvent>? ResumeTree(EventNodeInst<TState, TEvent> eventNodeInst, Cursor<TState, TEvent> cursor)
    {
        eventNodeInst.Executor.Cursor = cursor;
        
        if (ShouldHandleStateUpdate(eventNodeInst, cursor))
        {
            cursor.State = eventNodeInst.Executor.TryUpdateState(cursor.CurrentEvent);
        }
        
        if (cursor.InitEvents.Count == 1 && eventNodeInst.Executor.HandlesEvents.Contains(cursor.CurrentEvent.GetType()))
        {
            return eventNodeInst;
        }
        
        List<EventNodeInst<TState, TEvent>> nextExecutors = [..eventNodeInst.NextExecutors, eventNodeInst];

        // TODO maybe single is not required here and should be FirstOrDefault as it is guaranteed by tree validation for node to handle the same event as other sibling nodes
        var nextExecutor = nextExecutors.SingleOrDefault(ne => ne.Executor.HandlesEvents.Contains(cursor.CurrentEvent.GetType()));

        if (nextExecutor is null)
        {
            return null;
        }
        
        if (cursor.InitEvents.Count > 1)
        {
            PopProcessedEvent(cursor);
        }

        return ResumeTree(nextExecutor, cursor);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cursor"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="EventSourceEngineResumeException">Thrown when could not find a node that can handle the latest event</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    private async Task<ExecuteTreeResult<TState, TEvent>> Resume(Cursor<TState, TEvent> cursor, CancellationToken cancellationToken)
    {
        var eventNodeInst = FindNodeToExecuteAndRecreateState(cursor);
        
        if (eventNodeInst is null)
        {
            throw new EventSourceEngineResumeException("Cannot resume event sourcing tree");
        }

        return await TryExecuteNode(eventNodeInst, cursor, cancellationToken);
    }

    private EventNodeInst<TState, TEvent>? FindNodeToExecuteAndRecreateState(Cursor<TState, TEvent> cursor)
    {
        if (cursor.ProcessedEvents.Count > 0)
        {
            if (!_eventNodeInst.Executor.HandlesEvents.Contains(cursor.ProcessedEvents.Peek().GetType()) || 
                !_eventNodeInst.Executor.ProducesEvents.Contains(cursor.CurrentEvent.GetType()))
                throw new EventSourceEngineResumeException("Cannot resume event sourcing tree");
        }
        
        var eventNodeInst = ResumeTree(_eventNodeInst, cursor);

        return eventNodeInst;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventNode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cursor"></param>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    private async Task<ExecuteTreeResult<TState, TEvent>> TryExecuteNode(
        EventNodeInst<TState, TEvent> eventNode,
        Cursor<TState, TEvent> cursor, 
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        eventNode.Executor.Cursor = cursor;
        
        if (eventNode.Executor.HandlesEvents.Contains(cursor.CurrentEvent.GetType()))
        {
            var generatedEvent = await eventNode.Executor.ExecuteAsync(cursor.CurrentEvent, cancellationToken);
            
            UpdateCursorWithNewEvent(generatedEvent, cursor);
            
            cursor.State = eventNode.Executor.TryUpdateState(generatedEvent);
            
            await eventNode.Executor.AfterExecutionAndStateUpdate(generatedEvent, cancellationToken);
        }
        
        foreach (var nextExecutor in eventNode.NextExecutors)
        {
            nextExecutor.Executor.Cursor = cursor;
            
            if (nextExecutor.Executor.HandlesEvents.Contains(cursor.CurrentEvent.GetType()))
            {
                return await TryExecuteNode(nextExecutor, cursor, cancellationToken);
            }
        }
        
        return new ExecuteTreeResult<TState, TEvent>(cursor.State, cursor.CurrentEvent);
    }

    private EventNodeInst<TState, TEvent> InstantiateNode(EventNode<TState, TEvent> eventNode)
    {
        if (_serviceProvider.GetRequiredService(eventNode.Executor) is not INodeExecutor<TState, TEvent> nodeExecutor)
        {
            throw new Exception("Could not find provided object");
        }
        
        var eventNodeInst = new EventNodeInst<TState, TEvent>(
            nodeExecutor,
            eventNode.NextExecutors.Select(InstantiateNode).ToList());

        nodeExecutor.ProducesEvents = eventNode.ProducesEvents;
        nodeExecutor.HandlesEvents = eventNode.HandlesEvents;

        return eventNodeInst;
    }

    private static bool ShouldHandleStateUpdate(EventNodeInst<TState, TEvent> eventNodeInst, Cursor<TState, TEvent> cursor)
    {
        return eventNodeInst.Executor.ProducesEvents.Contains(cursor.CurrentEvent.GetType());
    }
    
    private static void UpdateCursorWithNewEvent(TEvent generatedEvent, Cursor<TState, TEvent> cursor)
    {
        PopProcessedEvent(cursor);
        cursor.InitEvents.Push(generatedEvent);
    }

    private static void PopProcessedEvent(Cursor<TState, TEvent> cursor)
    {
        var oldEvent = cursor.InitEvents.Pop();
        cursor.ProcessedEvents.Push(oldEvent);
    }
}