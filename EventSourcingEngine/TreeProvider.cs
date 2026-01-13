namespace EventSourcingEngine;

public abstract class TreeProvider<TState, TEvent> 
    where TState : new()
    where TEvent : Event
{
    private readonly IServiceProvider _serviceProvider;

    public TreeProvider(IServiceProvider serviceProvider)
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
        if (!typeof(INodeExecutor<TState, TEvent>).IsAssignableFrom(eventNode.Executor))
        {
            throw new Exception("dupa zbita");
        }
        
        if (_serviceProvider.GetService(eventNode.Executor) is null)
        {
            throw new Exception($"Executor has not been provided to DI");
        }

        CheckForDuplicatedHandledEventsInNextExecutor(eventNode);

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
                throw new Exception("asda");
            }
        }
    }
}