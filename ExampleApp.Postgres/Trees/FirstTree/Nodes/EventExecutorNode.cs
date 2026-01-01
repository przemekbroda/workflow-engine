using EventSourcingEngine;
using ExampleApp.Postgres.Models;

namespace ExampleApp.Postgres.Trees.FirstTree.Nodes;

public class EventExecutorNode(AppDbContext dbContext) : BaseNodeExecutor<TestState, FirstTreeEvent>
{
    public override async Task<FirstTreeEvent> ExecuteAsync(FirstTreeEvent e, CancellationToken cancellationToken)
    {
        var amount = 500;
        
        var dbEvent = new ProcessRequestEvent
        {
            EventName = nameof(ResultFetched),
            CreatedAt = DateTime.UtcNow,
            Index = e.Index + 1,
            ProcessRequestEventPayload = new ResultFetched(amount),
            ProcessRequestId = Cursor.State.ProcessId
        };
        
        dbContext.ProcessRequestEvents.Add(dbEvent);
        await dbContext.SaveChangesAsync();
        
        return new FirstTreeEvent.ResultFetched(amount, dbEvent.Index);
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