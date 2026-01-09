using EventSourcingEngine;
using ExampleApp.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace ExampleApp.Postgres.Trees.FirstTree.Nodes;

public class EventExecutorNode(AppDbContext dbContext) : BaseNodeExecutor<TestState, FirstTreeEvent>
{
    public override async Task<FirstTreeEvent> ExecuteAsync(FirstTreeEvent @event, CancellationToken cancellationToken)
    {
        //simulates some long task that can be canceled
        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        
        return new FirstTreeEvent.ResultFetched(500, @event.Index + 1);
    }

    protected override TestState UpdateState(FirstTreeEvent e)
    {
        return e switch
        {
            FirstTreeEvent.AwaitingResult => Cursor.State with { AwaitingResult = true },
            FirstTreeEvent.ResultFetched resultFetched => Cursor.State with
            {
                AwaitingResult = false, Balance = Cursor.State.Balance + resultFetched.Amount
            },
            _ => Cursor.State
        };
    }

    // we are not passing the cancellation token to those calls because if we finished some processing, we should save results to DB and just be done with this,
    // and we don't do the same actions again in later time.
    public override async Task AfterExecutionAndStateUpdate(FirstTreeEvent @event, CancellationToken _)
    {
        var dbEvent = ProcessRequestEvent.FromTreeEvent(@event, Cursor.State.ProcessRequestId, DateTime.UtcNow);
        dbContext.ProcessRequestEvents.Add(dbEvent);
        await dbContext.ProcessRequests.Where(r => r.Id == Cursor.State.ProcessRequestId)
            .ExecuteUpdateAsync(setter => setter.SetProperty(r => r.LastModifiedAt, DateTime.UtcNow), CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
}