using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingEngine;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterTree<TState, TEvent, TTreeProvider>(this IServiceCollection serviceCollection, ServiceLifetime eventSourceTreeLifetime = ServiceLifetime.Scoped)
        where TState : new()
        where TEvent : class
        where TTreeProvider : TreeProvider<TState, TEvent>
    {
        serviceCollection.AddSingleton<TreeProvider<TState, TEvent>, TTreeProvider>();
        
        var descriptor = new ServiceDescriptor(typeof(IEventSourceTree<TState, TEvent, TTreeProvider>), typeof(EventSourceTree<TState, TEvent, TTreeProvider>), eventSourceTreeLifetime);
        serviceCollection.Add(descriptor);
        
        return serviceCollection;
    }
}