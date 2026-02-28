using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingEngine;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterWorkflowTree<TState, TEvent, TTreeProvider>(this IServiceCollection serviceCollection, ServiceLifetime eventSourceTreeLifetime = ServiceLifetime.Scoped)
        where TState : class
        where TEvent : class
        where TTreeProvider : TreeProvider<TState, TEvent>
    {
        serviceCollection.AddSingleton<TTreeProvider>();
        
        var descriptor = new ServiceDescriptor(typeof(IWorkflowTree<TState, TEvent, TTreeProvider>), typeof(WorkflowTree<TState, TEvent, TTreeProvider>), eventSourceTreeLifetime);
        serviceCollection.Add(descriptor);
        
        return serviceCollection;
    }
}