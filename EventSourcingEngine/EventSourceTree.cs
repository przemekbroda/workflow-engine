using EventSourcingEngine.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingEngine;

public abstract class EventSourceTree<TState> where TState : new()
{
    private EventNode<TState> _eventNode = null!;
    private EventNodeInst<TState> _eventNodeInst = null!;
    private readonly IServiceProvider _serviceProvider;
    private Cursor<TState> _cursor = null!;

    protected EventSourceTree(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        ResolveTree();
    }

    protected abstract EventNode<TState> ProvideTree();

    private void ResolveTree()
    {
        _eventNode = ProvideTree();

        if (_eventNode is null)
        {
            throw new EventSourcingEngineException("No nodes were provided for event sourcing engine");
        }
        
        _eventNodeInst = InstantiateNode(_eventNode);
    }

    public void SetupCursor(List<Event> existingEvents)
    {
        var ordered = existingEvents
            .ToList();

        ordered.Reverse();

        var stack = new Stack<Event>(ordered);

        var treeCursor = new Cursor<TState>
        {
            State = new TState(),
            InitEvents = stack
        };

        _cursor = treeCursor;
    }

    private void  InitializeState(Func<object?, TState> stateInitializer)
    {
        _cursor.State = stateInitializer(_cursor.CurrentEvent.Payload);

        //if tree only contains one init event - initial event, don't pop it from the stack
        if (_cursor.InitEvents.Count > 1)
        {
            PopProcessedEvent();
        }
    }
    
    private async Task<EventNodeInst<TState>?> ResumeTree(EventNodeInst<TState> eventNodeInst, CancellationToken cancellationToken)
    {
        eventNodeInst.Executor.Cursor = _cursor;

        switch (_cursor.InitEvents.Count)
        {
            //if this is the last event on the list, then don't restore the state because that's the node that should handle the event
            case 1 when eventNodeInst.Executor.HandlesEvents.Contains(_cursor.CurrentEvent.EventName):
                return eventNodeInst;
            case 1:
                return null;
        }
        
        if (ShouldHandleStateUpdate(eventNodeInst))
        {
            Console.WriteLine($"Updating state in the {eventNodeInst.Executor.GetType().Name}");
            await eventNodeInst.Executor.TryUpdateState(_cursor.CurrentEvent, cancellationToken);

            PopProcessedEvent();

            if (ShouldHandleStateUpdate(eventNodeInst))
            {
                return await ResumeTree(eventNodeInst, cancellationToken);
            }

            foreach (var nextExecutor in _eventNodeInst.NextExecutors)
            {
                var toResume = await ResumeTree(nextExecutor, cancellationToken);
                if (toResume is not null)
                {
                    return toResume;
                }
            }

            return null;
        }

        return eventNodeInst;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="stateInitializer">
    ///     Function that initializes the state, argument of the func is the payload of the first event.
    ///     It is being executed only once just after ExecuteTree method call. If first event's payload is null,
    ///     then passed object to the function will also be null
    /// </param>
    /// <exception cref="Exception"></exception>
    public async Task ExecuteTree(Func<object?, TState> stateInitializer, CancellationToken cancellationToken)
    {
        InitializeState(stateInitializer);
        
        var eventNodeInst = await ResumeTree(_eventNodeInst, cancellationToken);

        if (eventNodeInst is null)
        {
            throw new Exception("Cannot resume event sourceing tree");
        }
        
        await TryExecuteNode(eventNodeInst, cancellationToken);
        
        Console.WriteLine("finished processing");
    }

    private async Task TryExecuteNode(EventNodeInst<TState> eventNode, CancellationToken cancellationToken)
    {
        eventNode.Executor.Cursor = _cursor;
        
        if (eventNode.Executor.HandlesEvents.Contains(_cursor.CurrentEvent.EventName))
        {
            var generatedEvent = await eventNode.Executor.ExecuteAsync(_cursor.CurrentEvent, cancellationToken);

            UpdateCursorWithNewEvent(generatedEvent);

            await eventNode.Executor.TryUpdateState(generatedEvent, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();

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

    private void UpdateCursorWithNewEvent(Event generatedEvent)
    {
        PopProcessedEvent();
        _cursor.InitEvents.Push(generatedEvent);
    }

    private void PopProcessedEvent()
    {
        var oldEvent = _cursor.InitEvents.Pop();
        _cursor.ProcessedEvents.Push(oldEvent);
    }

    private EventNodeInst<TState> InstantiateNode(EventNode<TState> eventNode)
    {
        ValidateNodeType(eventNode);

        if (_serviceProvider.GetRequiredService(eventNode.Executor) is not INodeExecutor<TState> nodeExecutor)
        {
            throw new Exception("Could not find provided object");
        }
        
        var eventNodeInst = new EventNodeInst<TState>(
            nodeExecutor,
            eventNode.NextExecutors.Select(InstantiateNode).ToList());

        nodeExecutor.ProducesEvents = eventNode.ProducesEvents;
        nodeExecutor.HandlesEvents = eventNode.HandlesEvents;

        return eventNodeInst;
    }

    private static void ValidateNodeType(EventNode<TState> eventNode)
    {
        if (!typeof(INodeExecutor<TState>).IsAssignableFrom(eventNode.Executor))
        {
            throw new Exception("dupa zbita");
        }

        //check for duplicated handled events in next executors
        var eventNames = new HashSet<string>();
        foreach (var producesEventName in eventNode.NextExecutors.Select(ne => ne.HandlesEvents).SelectMany(x => x))
        {
            if (!eventNames.Add(producesEventName))
            {
                throw new Exception("asda");
            }
        }
    }

    private bool ShouldHandleStateUpdate(EventNodeInst<TState> eventNodeInst)
    {
        // probably not needed
        // var previousEventExists = _cursor.ProcessedEvents.TryPeek(out var previousEvent);
        // var handlesLastProcessedEvent = eventNodeInst.Executor.HandlesEvents.Contains(previousEventExists ? previousEvent!.EventName : _cursor.CurrentEvent.EventName);

        var producesEvent = eventNodeInst.Executor.ProducesEvents.Contains(_cursor.CurrentEvent.EventName);
        
        return producesEvent;
    }
}