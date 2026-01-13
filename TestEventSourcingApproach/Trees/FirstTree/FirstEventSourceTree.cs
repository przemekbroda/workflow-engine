using EventSourcingEngine;
using TestEventSourcingApproach.Trees.FirstTree.Nodes;

namespace TestEventSourcingApproach.Trees.FirstTree;

public sealed class FirstEventSourceTree(IServiceProvider serviceProvider) : EventSourceTree<TestState>(serviceProvider)
{
    protected override EventNode<TestState> ProvideTree()
    {
        return new EventNode<TestState>(
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
                new EventNode<TestState>(
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