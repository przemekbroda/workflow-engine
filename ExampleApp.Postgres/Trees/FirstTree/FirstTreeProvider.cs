using EventSourcingEngine;
using ExampleApp.Postgres.Trees.FirstTree.Nodes;

namespace ExampleApp.Postgres.Trees.FirstTree;

public class FirstTreeProvider : TreeProvider<TestState, FirstTreeEvent>
{
    public override EventNode<TestState, FirstTreeEvent> ProvideTree()
    {
        return new EventNode<TestState, FirstTreeEvent>
        {
            HandlesEvents = [
                typeof(FirstTreeEvent.AwaitingExecution),
                typeof(FirstTreeEvent.AwaitingResult)
            ],
            Executor = typeof(EventExecutorNode),
            ProducesEvents = [
                typeof(FirstTreeEvent.AwaitingResult),
                typeof(FirstTreeEvent.ResultFetched)
            ],
            NextExecutors = [
                new EventNode<TestState, FirstTreeEvent>
                {
                    HandlesEvents = [
                        typeof(FirstTreeEvent.ResultFetched),
                        typeof(FirstTreeEvent.ResultSaveError)
                    ],
                    Executor = typeof(ResultSaverNode),
                    ProducesEvents = [
                        typeof(FirstTreeEvent.ResultSaved),
                        typeof(FirstTreeEvent.ResultSaveError),
                    ],
                    NextExecutors = []
                }
            ]
        };
    }
}