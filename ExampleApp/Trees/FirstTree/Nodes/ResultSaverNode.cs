using EventSourcingEngine;
using ExampleApp.Trees.FirstTree.Payloads;

namespace ExampleApp.Trees.FirstTree.Nodes;

public class ResultSaverNode : BaseNodeExecutor<TestState, FirstTreeEvent>
{
    public override async Task<FirstTreeEvent> ExecuteAsync(FirstTreeEvent @event, CancellationToken cancellationToken)
    {
        switch (@event)
        {
            case FirstTreeEvent.ResultFetched:
                Console.WriteLine($"Amount: {Cursor.State.Balance}");
                return new FirstTreeEvent.ResultSaveError();
            case FirstTreeEvent.ResultSaveError:
                return new FirstTreeEvent.ResultSaved();
            default:
                throw new Exception($"unhandled event: {@event.GetType().Name}");
        }
    }

    protected override void UpdateState(FirstTreeEvent e)
    {
        if (e is FirstTreeEvent.ResultSaveError)
        {
            
        }
    }
}