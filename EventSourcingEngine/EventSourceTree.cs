using EventSourcingEngine.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventSourcingEngine;

internal class EventSourceTree<TState, TEvent> : IEventSourceTree<TState, TEvent>
    where TState : new()
    where TEvent : Event
{
    private readonly EventNode<TState, TEvent> _eventNode;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventSourceTree<TState, TEvent>> _logger;
    
    private EventNodeInst<TState, TEvent> _eventNodeInst = null!;
    private Cursor<TState, TEvent> _cursor = null!;

    public EventSourceTree(
        IServiceProvider serviceProvider, 
        TreeProvider<TState, TEvent> treeProvider,
        ILogger<EventSourceTree<TState, TEvent>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _eventNode = treeProvider.ProvideTree();
        ResolveTree();
    }
    
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
    public async Task<Event> ExecuteTree(IEnumerable<TEvent> initialCursorEvents, Func<object?, TState> stateInitializer, CancellationToken cancellationToken)
    {
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

    private Cursor<TState, TEvent> SetupCursor(IEnumerable<TEvent> existingEvents, Func<object?, TState> stateInitializer)
    {
        var treeCursor = new Cursor<TState, TEvent>
        {
            State = new TState(),
            InitEvents = new Stack<TEvent>(existingEvents)
        };

        treeCursor.State = stateInitializer(treeCursor.CurrentEvent.Payload);

        return treeCursor;
    }
    
    private EventNodeInst<TState, TEvent>? ResumeTree(EventNodeInst<TState, TEvent> eventNodeInst)
    {
        eventNodeInst.Executor.Cursor = _cursor;
        
        if (ShouldHandleStateUpdate(eventNodeInst))
        {
            eventNodeInst.Executor.TryUpdateState(_cursor.CurrentEvent);
        }
        
        if (_cursor.InitEvents.Count == 1 && eventNodeInst.Executor.HandlesEvents.Contains(_cursor.CurrentEvent.EventName))
        {
            return eventNodeInst;
        }
        
        List<EventNodeInst<TState, TEvent>> nextExecutors = [..eventNodeInst.NextExecutors, eventNodeInst];

        var nextExecutor = nextExecutors.SingleOrDefault(ne => ne.Executor.HandlesEvents.Contains(_cursor.CurrentEvent.EventName));

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

    private EventNodeInst<TState, TEvent>? TryResumeInNextExecutors(EventNodeInst<TState, TEvent> eventNodeInst)
    {
        foreach (var nextExecutor in eventNodeInst.NextExecutors)
        {
            var toResume = ResumeTree(nextExecutor);
            if (toResume is not null)
            {
                return toResume;
            }
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="EventSourceEngineResumeException">Thrown when could not find a node that can handle the latest event</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    private async Task<TEvent> Resume(CancellationToken cancellationToken)
    {
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
    private async Task<TEvent> TryExecuteNode(EventNodeInst<TState, TEvent> eventNode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        eventNode.Executor.Cursor = _cursor;
        
        if (eventNode.Executor.HandlesEvents.Contains(_cursor.CurrentEvent.EventName))
        {
            var generatedEvent = await eventNode.Executor.ExecuteAsync(_cursor.CurrentEvent, cancellationToken);

            UpdateCursorWithNewEvent(generatedEvent);

            eventNode.Executor.TryUpdateState(generatedEvent);
        }
        
        foreach (var nextExecutor in eventNode.NextExecutors)
        {
            nextExecutor.Executor.Cursor = _cursor;
            
            if (nextExecutor.Executor.HandlesEvents.Contains(_cursor.CurrentEvent.EventName))
            {
                return await TryExecuteNode(nextExecutor, cancellationToken);
            }
        }
        
        return _cursor.CurrentEvent;
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
        return eventNodeInst.Executor.ProducesEvents.Contains(_cursor.CurrentEvent.EventName);
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