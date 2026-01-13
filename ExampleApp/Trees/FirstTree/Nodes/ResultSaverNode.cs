using EventSourcingEngine;
using ExampleApp.Trees.FirstTree.Payloads;

namespace ExampleApp.Trees.FirstTree.Nodes;

public class ResultSaverNode : BaseNodeExecutor<TestState, FirstTreeEvent>
{
    public override async Task<FirstTreeEvent> ExecuteAsync(FirstTreeEvent @event, TestState state, CancellationToken cancellationToken)
    {
        switch (@event)
        {
            case FirstTreeEvent.ResultFetched:
                Console.WriteLine($"Amount: {state.Balance}");
                return new FirstTreeEvent.ResultSaveError();
            case FirstTreeEvent.ResultSaveError:
                return new FirstTreeEvent.ResultSaved();
            default:
                throw new Exception($"unhandled event: {@event.GetType().Name}");
        }
    }

    protected override TestState UpdateState(FirstTreeEvent e, TestState state)
    {
        if (e is FirstTreeEvent.ResultSaveError)
        {
            
        }
        
        return state;
    }
}