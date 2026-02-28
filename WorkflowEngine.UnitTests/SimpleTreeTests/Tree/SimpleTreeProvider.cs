using EventSourcingEngine.UnitTests.SimpleTreeTests.Nodes;

namespace EventSourcingEngine.UnitTests.SimpleTreeTests.Tree;

public class SimpleTreeProvider : TreeProvider<SimpleTreeState, SimpleTreeEvent>
{
    public override EventNode<SimpleTreeState, SimpleTreeEvent> ProvideTree()
    {
        return new EventNode<SimpleTreeState, SimpleTreeEvent>
        {
            HandlesEvents = [
                typeof(SimpleTreeEvent.AwaitingExecution),
                typeof(SimpleTreeEvent.AwaitingResult)
            ],
            Executor = typeof(FirstEventExecutorNode),
            ProducesEvents = [
                typeof(SimpleTreeEvent.AwaitingResult),
                typeof(SimpleTreeEvent.ResultFetched)
            ],
            NextExecutors = [
                new EventNode<SimpleTreeState, SimpleTreeEvent>
                {
                    HandlesEvents = [
                        typeof(SimpleTreeEvent.ResultFetched),
                        typeof(SimpleTreeEvent.ResultSaveError)
                    ],
                    Executor = typeof(ResultSaverNode),
                    ProducesEvents = [
                        typeof(SimpleTreeEvent.ResultSaved),
                        typeof(SimpleTreeEvent.ResultSaveError),
                    ],
                    NextExecutors = []
                }
            ]
        };
    }
}