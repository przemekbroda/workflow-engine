using EventSourcingEngine.UnitTests.SimpleTreeTests.Tree;

namespace EventSourcingEngine.UnitTests.SimpleTreeTests.Nodes;

public class FirstEventExecutorNode : BaseNodeExecutor<SimpleTreeState, SimpleTreeEvent>
{
    public override async Task<SimpleTreeEvent> ExecuteAsync(SimpleTreeEvent @event, CancellationToken cancellationToken)
    {
        return new SimpleTreeEvent.ResultFetched(500);
    }

    protected override SimpleTreeState UpdateState(SimpleTreeEvent e)
    {
        return e switch
        {
            SimpleTreeEvent.AwaitingResult => Cursor.State with
            {
                AwaitingResult = true,
                Attempt = Cursor.State.Attempt + 1
            },
            SimpleTreeEvent.ResultFetched resultFetched => Cursor.State with
            {
                AwaitingResult = false, Balance = Cursor.State.Balance + resultFetched.Amount
            },
            _ => Cursor.State
        };
    }
}