using EventSourcingEngine;

namespace ExampleApp.Trees.FirstTree.Nodes;

public class EventExecutorNode : BaseNodeExecutor<TestState, FirstTreeEvent>
{
    public override async Task<FirstTreeEvent> ExecuteAsync(FirstTreeEvent @event, CancellationToken cancellationToken)
    {
        return new FirstTreeEvent.ResultFetched(500);
    }

    protected override TestState UpdateState(FirstTreeEvent e)
    {
        return e switch
        {
            FirstTreeEvent.AwaitingResult => Cursor.State with { AwaitingResult = true },
            FirstTreeEvent.ResultFetched resultFetched => Cursor.State with
            {
                AwaitingResult = false, Balance = Cursor.State.Balance + resultFetched.Amount
            },
            _ => Cursor.State
        };
    }
}