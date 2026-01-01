using EventSourcingEngine;

namespace ExampleApp.Postgres.Trees.FirstTree.Nodes;

public class ResultSaverNode(AppDbContext dbContext) : BaseNodeExecutor<TestState, FirstTreeEvent>
{
    public override async Task<FirstTreeEvent> ExecuteAsync(FirstTreeEvent e, CancellationToken cancellationToken)
    {
        if (e is FirstTreeEvent.ResultFetched resultFetched)
        {
            Console.WriteLine($"Amount: {Cursor.State.Balance}");
            return new FirstTreeEvent.ResultSaveError();
        }
        
        if (e is FirstTreeEvent.ResultSaveError)
        {
            return new FirstTreeEvent.ResultSaved();
        }

        throw new Exception($"unhandled event: {e.GetType().Name}");
    }

    protected override void UpdateState(FirstTreeEvent e)
    {
        if (e is FirstTreeEvent.ResultSaveError)
        {
            
        }
    }
}