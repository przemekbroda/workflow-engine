using EventSourcingEngine;
using ExampleApp.Trees.FirstTree.Payloads;

namespace ExampleApp.Trees.FirstTree.Nodes;

public class ResultSaverNode : BaseNodeExecutor<TestState, Event>
{
    public override async Task<Event> ExecuteAsync(Event e, CancellationToken cancellationToken)
    {
        if (e.EventName == "ResultFetched")
        {
            Console.WriteLine($"Amount: {Cursor.State.Balance}");
            return new Event("ResultSaveError", new ResultSaveErrorPayload());
        }

        if (e.EventName == "ResultSaveError")
        {
            return new Event("ResultSaved", null);
        }

        throw new Exception($"unhandled event: {e.EventName}");
    }

    protected override void UpdateState(Event e)
    {
        if (e.EventName == "ResultSaveError")
        {
            
        }
    }
}