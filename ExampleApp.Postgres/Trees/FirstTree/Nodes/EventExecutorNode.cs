using EventSourcingEngine;
using ExampleApp.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace ExampleApp.Postgres.Trees.FirstTree.Nodes;

public class EventExecutorNode(AppDbContext dbContext) : BaseNodeExecutor<TestState, FirstTreeEvent>
{
    public override async Task<FirstTreeEvent> ExecuteAsync(FirstTreeEvent e, CancellationToken cancellationToken)
    {
        var amount = 500;

        await Task.Delay(TimeSpan.FromSeconds(10));
        
        var dbEvent = new ProcessRequestEvent
        {
            EventName = nameof(FirstTreeEvent.ResultFetched),
            CreatedAt = DateTime.UtcNow,
            Index = e.Index + 1,
            ProcessRequestEventPayload = new ResultFetched(amount),
            ProcessRequestId = Cursor.State.ProcessRequestId
        };

        dbContext.ProcessRequestEvents.Add(dbEvent);
        
        // we are not passing the cancellation token to those calls because if we finished some processing, we should save results to DB
        // so we don't do the same actions again in later time
        await dbContext.ProcessRequests.Where(r => r.Id == Cursor.State.ProcessRequestId)
            .ExecuteUpdateAsync(setter => setter.SetProperty(r => r.LastModifiedAt, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();
        
        return dbEvent.GetTreeEvent();
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