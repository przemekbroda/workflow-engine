using EventSourcingEngine.Exceptions;

namespace EventSourcingEngine;

public abstract class TreeProvider<TState, TEvent> 
    where TState : new()
    where TEvent : Event
{
    private readonly IServiceProvider _serviceProvider;

    protected TreeProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        ValidateTree();
    }

    public abstract EventNode<TState, TEvent> ProvideTree();

    private void ValidateTree()
    {
        var eventNode = ProvideTree();
        
        ValidateNodeType(eventNode);
    }
    
    private void ValidateNodeType(EventNode<TState, TEvent> eventNode)
    {
        if (eventNode.HandlesEvents.Count == 0)
        {
            throw new EventSourcingEngineTreeValidationException("Node must handle at least one event");
        }

        if (eventNode.ProducesEvents.Count == 0)
        {
            throw new EventSourcingEngineTreeValidationException("Node must produce at least one event");
        }
        
        if (!typeof(INodeExecutor<TState, TEvent>).IsAssignableFrom(eventNode.Executor))
        {
            throw new EventSourcingEngineTreeValidationException("Executor must implement INodeExecutor");
        }
        
        if (_serviceProvider.GetService(eventNode.Executor) is null)
        {
            throw new EventSourcingEngineTreeValidationException($"Executor {eventNode.Executor.Name} has not been provided to DI");
        }

        CheckForDuplicatedHandledEventsInNextExecutor(eventNode);
        CheckNextExecutorsHandleProducedEvents(eventNode);
        
        foreach (var nextExecutor in eventNode.NextExecutors)
        {
            ValidateNodeType(nextExecutor);
        }
    }

    private static void CheckForDuplicatedHandledEventsInNextExecutor(EventNode<TState, TEvent> eventNode)
    {
        var eventNames = new HashSet<string>();
        foreach (var producesEventName in eventNode.NextExecutors.Select(ne => ne.HandlesEvents).SelectMany(x => x))
        {
            if (!eventNames.Add(producesEventName))
            {
                throw new EventSourcingEngineTreeValidationException($"Child nodes handles same event ({producesEventName}) as other node with the same parent node");
            }
        }
    }

    private static void CheckNextExecutorsHandleProducedEvents(EventNode<TState, TEvent> parentNode)
    {
        var parentProducedEvents = parentNode.ProducesEvents;
        
        foreach (var childNode in parentNode.NextExecutors)
        {
            foreach (var childNodeHandledEvent in childNode.HandlesEvents)
            {
                if (!parentProducedEvents.Remove(childNodeHandledEvent) && !childNode.ProducesEvents.Contains(childNodeHandledEvent))
                {
                    throw new EventSourcingEngineTreeValidationException($"Node with an executor {childNode.Executor.Name} handles event {childNodeHandledEvent} that is not produced by parent node with an executor {parentNode.Executor.Name} or by itself");
                }
            }
        }
    }
}