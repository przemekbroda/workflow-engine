using EventSourcingEngine;
using TestEventSourcingApproach.Trees.FirstTree.Nodes;

namespace TestEventSourcingApproach.Trees.FirstTree;

public sealed class FirstEventSourceTree(IServiceProvider serviceProvider) : EventSourceTree<TestState, Event>(serviceProvider)
{
    protected override EventNode<TestState, Event> ProvideTree()
    {
        return new EventNode<TestState, Event>(
            [
                "AwaitingExecution",
                "AwaitingResult"
            ],
            typeof(EventExecutorNode), 
            [
                "AwaitingResult",
                "ResultFetched"
            ],
            [
                new EventNode<TestState, Event>(
                    [
                        "ResultFetched",
                        "ResultSaveError"
                    ],
                    typeof(ResultSaverNode), 
                    [
                        "ResultSaved",
                        "ResultSaveError",
                    ],
                    []
                    )
            ]
            );
    }
}