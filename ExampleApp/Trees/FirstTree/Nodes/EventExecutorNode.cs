using EventSourcingEngine;

namespace ExampleApp.Trees.FirstTree.Nodes;

public class EventExecutorNode : BaseNodeExecutor<TestState, FirstTreeEvent>
{
    public override async Task<FirstTreeEvent> ExecuteAsync(FirstTreeEvent e, CancellationToken cancellationToken)
    {
        return new FirstTreeEvent.ResultFetched(500);
    }

    protected override void UpdateState(FirstTreeEvent e)
    {
        switch (e)
        {
            case FirstTreeEvent.AwaitingResult:
                Cursor.State.AwaitingResult = true;
                break;
            case FirstTreeEvent.ResultFetched resultFetched:
                Cursor.State.AwaitingResult = false;
                Cursor.State.Balance += resultFetched.Amount;
                break;
        }
    }
}