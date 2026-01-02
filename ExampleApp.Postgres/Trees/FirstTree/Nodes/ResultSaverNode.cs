using EventSourcingEngine;
using ExampleApp.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace ExampleApp.Postgres.Trees.FirstTree.Nodes;

public class ResultSaverNode(AppDbContext dbContext) : BaseNodeExecutor<TestState, FirstTreeEvent>
{
    public override async Task<FirstTreeEvent> ExecuteAsync(FirstTreeEvent @event, CancellationToken _)
    {
        if (@event is FirstTreeEvent.ResultFetched)
        {
            return new FirstTreeEvent.ResultSaveError(@event.Index + 1);
        }
        
        if (@event is FirstTreeEvent.ResultSaveError)
        {
            return new FirstTreeEvent.ResultSaved(@event.Index + 1);
        }
        
        throw new Exception($"unhandled event: {@event.GetType().Name}");
    }

    protected override void UpdateState(FirstTreeEvent e)
    {
        if (e is FirstTreeEvent.ResultSaveError)
        {
            
        }
    }

    // we are not passing the cancellation token to those calls because if we finished some processing, we should save results to DB
    // so we don't do the same actions again in later time
    public override async Task AfterExecutionAndStateUpdate(FirstTreeEvent @event, CancellationToken _)
    {
        var dbEvent = ProcessRequestEvent.FromTreeEvent(@event, Cursor.State.ProcessRequestId, DateTime.UtcNow);
        dbContext.ProcessRequestEvents.Add(dbEvent);
        await dbContext.ProcessRequests.Where(r => r.Id == Cursor.State.ProcessRequestId)
            .ExecuteUpdateAsync(setter => setter.SetProperty(r => r.LastModifiedAt, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();
    }
}