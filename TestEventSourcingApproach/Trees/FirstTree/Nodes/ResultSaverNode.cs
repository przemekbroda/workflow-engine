using EventSourcingEngine;
using TestEventSourcingApproach.Trees.FirstTree.Payloads;

namespace TestEventSourcingApproach.Trees.FirstTree.Nodes;

public class ResultSaverNode : BaseNodeExecutor<TestState>
{
    public override async Task<Event> ExecuteAsync(Event e, CancellationToken cancellationToken)
    {
        if (e.EventName == "ResultFetched")
        {
            Console.WriteLine($"Amount: {Cursor.State.Balance}");
            return new Event("ResultSaveError", new ResultSaveErrorPayload());
        }
        else if (e.EventName == "ResultSaveError")
        {
            return new Event("ResultSaved", null);
        }

        throw new Exception($"unhandled event: {e.EventName}");
    }

    public override async Task UpdateState(Event e, CancellationToken cancellationToken)
    {
        if (e.EventName == "ResultSaveError")
        {
            
        }
    }
}