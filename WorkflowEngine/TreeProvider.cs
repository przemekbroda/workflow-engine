using EventSourcingEngine.Exceptions;

namespace EventSourcingEngine;

public abstract class TreeProvider<TState, TEvent> 
    where TState : class
    where TEvent : class
{
    internal HashSet<Type> HandledEvents { get; } = [];
    
    protected TreeProvider()
    {
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
            throw new WorkflowEngineTreeValidationException("Node must handle at least one event");
        }

        if (eventNode.ProducesEvents.Count == 0)
        {
            throw new WorkflowEngineTreeValidationException("Node must produce at least one event");
        }
        
        if (!typeof(INodeExecutor<TState, TEvent>).IsAssignableFrom(eventNode.Executor))
        {
            throw new WorkflowEngineTreeValidationException("Executor must implement INodeExecutor");
        }

        CheckForDuplicatedHandledEventsInNextExecutor(eventNode);
        CheckNextExecutorsHandleProducedEvents(eventNode);
        
        HandledEvents.UnionWith(eventNode.HandlesEvents);
        
        foreach (var nextExecutor in eventNode.NextExecutors)
        {
            ValidateNodeType(nextExecutor);
        }
    }

    private static void CheckForDuplicatedHandledEventsInNextExecutor(EventNode<TState, TEvent> eventNode)
    {
        var eventTypes = new HashSet<Type>();
        foreach (var producesEventName in eventNode.NextExecutors.Select(ne => ne.HandlesEvents).SelectMany(x => x))
        {
            if (!eventTypes.Add(producesEventName))
            {
                throw new WorkflowEngineTreeValidationException($"Child node handles same event ({producesEventName}) as other node with the same parent node");
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
                    throw new WorkflowEngineTreeValidationException($"Node with an executor {childNode.Executor.Name} handles event {childNodeHandledEvent} that is not produced by parent node with an executor {parentNode.Executor.Name} or by itself");
                }
            }
        }
    }
}