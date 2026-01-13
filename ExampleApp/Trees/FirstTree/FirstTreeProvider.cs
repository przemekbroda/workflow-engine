using EventSourcingEngine;
using ExampleApp.Trees.FirstTree.Nodes;

namespace ExampleApp.Trees.FirstTree;

public class FirstTreeProvider(IServiceProvider serviceProvider) : TreeProvider<TestState, Event>(serviceProvider)
{
    public override EventNode<TestState, Event> ProvideTree()
    {
        return new EventNode<TestState, Event>
        {
            HandlesEvents = [
                "AwaitingExecution",
                "AwaitingResult"
            ],
            Executor = typeof(EventExecutorNode),
            ProducesEvents = [
                "AwaitingResult",
                "ResultFetched"
            ],
            NextExecutors = [
                new EventNode<TestState, Event>
                {
                    HandlesEvents = [
                        "ResultFetched",
                        "ResultSaveError"
                    ],
                    Executor = typeof(ResultSaverNode),
                    ProducesEvents = [
                        "ResultSaved",
                        "ResultSaveError",
                    ],
                    NextExecutors = []
                }
            ]
        };
    }
}