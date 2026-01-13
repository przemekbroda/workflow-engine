using EventSourcingEngine.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventSourcingEngine;

internal class EventSourceTree<TState, TEvent, TTreeProvider> : IEventSourceTree<TState, TEvent, TTreeProvider>
    where TState : struct
    where TEvent : class
    where TTreeProvider : TreeProvider<TState, TEvent>
{
    private readonly EventNode<TState, TEvent> _eventNode;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventSourceTree<TState, TEvent, TTreeProvider>> _logger;
    private readonly TreeProvider<TState, TEvent> _treeProvider;
    
    private EventNodeInst<TState, TEvent> _eventNodeInst = null!;
    private Cursor<TState, TEvent> _cursor = null!;

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
    /// <param name="initialCursorEvents"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="stateInitializer">
    ///     Function that initializes the state, argument of the func is the payload of the first event.
    ///     It is being executed only once just after the ExecuteTree method call. If the first event's payload is null,
    ///     then a passed object to the function will also be null
    /// </param>
    public async Task<ExecuteTreeResult<TState, TEvent>> ExecuteTree(IList<TEvent> initialCursorEvents, Func<TEvent, TState> stateInitializer, CancellationToken cancellationToken)
    {
        ValidateInitialCursorEvents(initialCursorEvents);

        _cursor = SetupCursor(initialCursorEvents, stateInitializer);
        
        //if the tree only contains one init event - initial event, don't pop it from the stack
        if (_cursor.InitEvents.Count > 1)
        {
            PopProcessedEvent();
        }

        var finishedWithEvent = await Resume(cancellationToken);
        
        _logger.LogDebug("Finished executing event sourcing tree");

        return finishedWithEvent;
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
    
    private EventNodeInst<TState, TEvent>? ResumeTree(EventNodeInst<TState, TEvent> eventNodeInst)
    {
        eventNodeInst.Executor.Cursor = _cursor;
        
        if (ShouldHandleStateUpdate(eventNodeInst))
        {
            _cursor.State = eventNodeInst.Executor.TryUpdateState(_cursor.CurrentEvent, _cursor.State);
        }
        
        if (_cursor.InitEvents.Count == 1 && eventNodeInst.Executor.HandlesEvents.Contains(_cursor.CurrentEvent.GetType()))
        {
            return eventNodeInst;
        }
        
        List<EventNodeInst<TState, TEvent>> nextExecutors = [..eventNodeInst.NextExecutors, eventNodeInst];

        // TODO maybe single is not required here and should be FirstOrDefault as it is guaranteed by tree validation for node to handle the same event as other sibling nodes
        var nextExecutor = nextExecutors.SingleOrDefault(ne => ne.Executor.HandlesEvents.Contains(_cursor.CurrentEvent.GetType()));

        if (nextExecutor is null)
        {
            return null;
        }
        
        if (_cursor.InitEvents.Count > 1)
        {
            PopProcessedEvent();
        }

        return ResumeTree(nextExecutor);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="EventSourceEngineResumeException">Thrown when could not find a node that can handle the latest event</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    private async Task<ExecuteTreeResult<TState, TEvent>> Resume(CancellationToken cancellationToken)
    {
        if (_cursor.ProcessedEvents.Count > 0)
        {
            if (!_eventNodeInst.Executor.HandlesEvents.Contains(_cursor.ProcessedEvents.Peek().GetType()) || 
                !_eventNodeInst.Executor.ProducesEvents.Contains(_cursor.CurrentEvent.GetType()))
                throw new EventSourceEngineResumeException("Cannot resume event sourcing tree");
        }
        
        var eventNodeInst = ResumeTree(_eventNodeInst);

        if (eventNodeInst is null)
        {
            throw new EventSourceEngineResumeException("Cannot resume event sourcing tree");
        }
        
        return await TryExecuteNode(eventNodeInst, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventNode"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    private async Task<ExecuteTreeResult<TState, TEvent>> TryExecuteNode(EventNodeInst<TState, TEvent> eventNode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        eventNode.Executor.Cursor = _cursor;
        
        if (eventNode.Executor.HandlesEvents.Contains(_cursor.CurrentEvent.GetType()))
        {
            var generatedEvent = await eventNode.Executor.ExecuteAsync(_cursor.CurrentEvent, _cursor.State, cancellationToken);

            UpdateCursorWithNewEvent(generatedEvent);

            _cursor.State = eventNode.Executor.TryUpdateState(generatedEvent, _cursor.State);
            
            await eventNode.Executor.AfterExecutionAndStateUpdate(generatedEvent, _cursor.State, cancellationToken);
        }
        
        foreach (var nextExecutor in eventNode.NextExecutors)
        {
            nextExecutor.Executor.Cursor = _cursor;
            
            if (nextExecutor.Executor.HandlesEvents.Contains(_cursor.CurrentEvent.GetType()))
            {
                return await TryExecuteNode(nextExecutor, cancellationToken);
            }
        }
        
        return new ExecuteTreeResult<TState, TEvent>(_cursor.State, _cursor.CurrentEvent);
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

    private bool ShouldHandleStateUpdate(EventNodeInst<TState, TEvent> eventNodeInst)
    {
        return eventNodeInst.Executor.ProducesEvents.Contains(_cursor.CurrentEvent.GetType());
    }
    
    private void UpdateCursorWithNewEvent(TEvent generatedEvent)
    {
        PopProcessedEvent();
        _cursor.InitEvents.Push(generatedEvent);
    }

    private void PopProcessedEvent()
    {
        var oldEvent = _cursor.InitEvents.Pop();
        _cursor.ProcessedEvents.Push(oldEvent);
    }
}