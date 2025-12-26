using EventSourcingEngine.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingEngine;

internal class EventSourceTree<TState, TEvent> : IEventSourceTree<TState, TEvent>
    where TState : new()
    where TEvent : Event
{
    private readonly EventNode<TState, TEvent> _eventNode;
    private readonly IServiceProvider _serviceProvider;
    
    private EventNodeInst<TState, TEvent> _eventNodeInst = null!;
    private Cursor<TState, TEvent> _cursor = null!;

    public EventSourceTree(IServiceProvider serviceProvider, TreeProvider<TState, TEvent> treeProvider)
    {
        _serviceProvider = serviceProvider;
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
    /// <exception cref="Exception"></exception>
    public async Task ExecuteTree(List<TEvent> initialCursorEvents, Func<object?, TState> stateInitializer, CancellationToken cancellationToken)
    {
        SetupCursor(initialCursorEvents);
        
        InitializeState(stateInitializer);

        await ResumeAndExecuteEventNode(cancellationToken);
        
        Console.WriteLine("finished processing");
    }
    
    private void ResolveTree()
    {
        if (_eventNode is null)
        {
            throw new EventSourcingEngineException("No nodes were provided for event sourcing engine");
        }
        
        _eventNodeInst = InstantiateNode(_eventNode);
    }

    private void SetupCursor(List<TEvent> existingEvents)
    {
        var ordered = existingEvents
            .ToList();

        ordered.Reverse();

        var stack = new Stack<TEvent>(ordered);

        var treeCursor = new Cursor<TState, TEvent>
        {
            State = new TState(),
            InitEvents = stack
        };

        _cursor = treeCursor;
    }

    private void InitializeState(Func<object?, TState> stateInitializer)
    {
        _cursor.State = stateInitializer(_cursor.CurrentEvent.Payload);

        //if the tree only contains one init event - initial event, don't pop it from the stack
        if (_cursor.InitEvents.Count > 1)
        {
            PopProcessedEvent();
        }
    }
    
    private EventNodeInst<TState, TEvent>? ResumeTree(EventNodeInst<TState, TEvent> eventNodeInst)
    {
        eventNodeInst.Executor.Cursor = _cursor;

        if (_cursor.InitEvents.Count == 1 && eventNodeInst.Executor.HandlesEvents.Contains(_cursor.CurrentEvent.EventName))
        {
            return eventNodeInst;
        }

        if (ShouldHandleStateUpdate(eventNodeInst))
        {
            Console.WriteLine($"Updating state in the {eventNodeInst.Executor.GetType().Name}");
            eventNodeInst.Executor.TryUpdateState(_cursor.CurrentEvent);

            if (_cursor.InitEvents.Count > 1)
            {
                PopProcessedEvent();
            }
                
            // It might be possible that same node should handle next event
            if (_cursor.InitEvents.Count > 1 && ShouldHandleStateUpdate(eventNodeInst))
            {
                return ResumeTree(eventNodeInst);
            }
            
            return TryResumeInNextExecutors(eventNodeInst);
        }
        
        return null;
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

    private async Task ResumeAndExecuteEventNode(CancellationToken cancellationToken)
    {
        var eventNodeInst = ResumeTree(_eventNodeInst);

        if (eventNodeInst is null)
        {
            throw new EventSourceEngineResumeException("Cannot resume event sourcing tree");
        }
        
        await TryExecuteNode(eventNodeInst, cancellationToken);
    }

    private async Task TryExecuteNode(EventNodeInst<TState, TEvent> eventNode, CancellationToken cancellationToken)
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
                await TryExecuteNode(nextExecutor, cancellationToken);
                break;
            }
        }
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
        var previousEvent = _cursor.ProcessedEvents.Peek();
        var handlesLastProcessedEvent = eventNodeInst.Executor.HandlesEvents.Contains(previousEvent.EventName);
        var producesEvent = eventNodeInst.Executor.ProducesEvents.Contains(_cursor.CurrentEvent.EventName);
        
        return producesEvent && handlesLastProcessedEvent;
    }
}