using EventSourcingEngine;
using TestEventSourcingApproach.Trees.FirstTree.Nodes;

namespace TestEventSourcingApproach.Trees.FirstTree;

public class FirstTreeProvider(IServiceProvider serviceProvider) : TreeProvider<TestState, Event>(serviceProvider)
{
    public override EventNode<TestState, Event> ProvideTree()
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