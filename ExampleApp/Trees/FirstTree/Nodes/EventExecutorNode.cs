using EventSourcingEngine;

namespace ExampleApp.Trees.FirstTree.Nodes;

public class EventExecutorNode : BaseNodeExecutor<TestState, FirstTreeEvent>
{
    public override async Task<FirstTreeEvent> ExecuteAsync(FirstTreeEvent @event, TestState state, CancellationToken cancellationToken)
    {
        return new FirstTreeEvent.ResultFetched(500);
    }

    protected override TestState UpdateState(FirstTreeEvent e, TestState state)
    {
        return e switch
        {
            FirstTreeEvent.AwaitingResult => state with { AwaitingResult = true },
            FirstTreeEvent.ResultFetched resultFetched => state with
            {
                AwaitingResult = false, Balance = state.Balance + resultFetched.Amount
            },
            _ => state
        };
    }
}