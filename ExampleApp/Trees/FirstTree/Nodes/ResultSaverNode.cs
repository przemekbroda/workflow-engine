using EventSourcingEngine;
using ExampleApp.Trees.FirstTree.Payloads;

namespace ExampleApp.Trees.FirstTree.Nodes;

public class ResultSaverNode : BaseNodeExecutor<TestState, FirstTreeEvent>
{
    public override async Task<FirstTreeEvent> ExecuteAsync(FirstTreeEvent @event, CancellationToken cancellationToken)
    {
        if (@event is FirstTreeEvent.ResultFetched resultFetched)
        {
            Console.WriteLine($"Amount: {Cursor.State.Balance}");
            return new FirstTreeEvent.ResultSaveError();
        }
        
        if (@event is FirstTreeEvent.ResultSaveError)
        {
            return new FirstTreeEvent.ResultSaved();
        }

        throw new Exception($"unhandled event: {@event.GetType().Name}");
    }

    protected override void UpdateState(FirstTreeEvent e)
    {
        if (e is FirstTreeEvent.ResultSaveError)
        {
            
        }
    }
}