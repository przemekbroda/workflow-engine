using EventSourcingEngine;
using ExampleApp.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace ExampleApp.Postgres.Trees.FirstTree.Nodes;

public class ResultSaverNode(AppDbContext dbContext) : BaseNodeExecutor<TestState, FirstTreeEvent>
{
    public override async Task<FirstTreeEvent> ExecuteAsync(FirstTreeEvent e, CancellationToken _)
    {
        if (e is FirstTreeEvent.ResultFetched)
        {
            Console.WriteLine($"Amount: {Cursor.State.Balance}");
            var dbEvent = new ProcessRequestEvent
            {
                EventName = nameof(FirstTreeEvent.ResultSaveError),
                CreatedAt = DateTime.UtcNow,
                Index = e.Index + 1,
                ProcessRequestId = Cursor.State.ProcessRequestId,
            };
            
            dbContext.ProcessRequestEvents.Add(dbEvent);
            
            // we are not passing the cancellation token to those calls because if we finished some processing, we should save results to DB
            // so we don't do the same actions again in later time
            await dbContext.ProcessRequests.Where(r => r.Id == Cursor.State.ProcessRequestId)
                .ExecuteUpdateAsync(setter => setter.SetProperty(r => r.LastModifiedAt, DateTime.UtcNow));
            await dbContext.SaveChangesAsync();
            
            return dbEvent.GetTreeEvent();
        }
        
        if (e is FirstTreeEvent.ResultSaveError)
        {
            var dbEvent = new ProcessRequestEvent
            {
                EventName = nameof(FirstTreeEvent.ResultSaved),
                CreatedAt = DateTime.UtcNow,
                Index = e.Index + 1,
                ProcessRequestId = Cursor.State.ProcessRequestId,
            };
            
            dbContext.ProcessRequestEvents.Add(dbEvent);
            
            // we are not passing the cancellation token to those calls because if we finished some processing, we should save results to DB
            // so we don't do the same actions again in later time
            await dbContext.ProcessRequests.Where(r => r.Id == Cursor.State.ProcessRequestId)
                .ExecuteUpdateAsync(setter => setter.SetProperty(r => r.LastModifiedAt, DateTime.UtcNow));
            await dbContext.SaveChangesAsync();
            
            return dbEvent.GetTreeEvent();
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