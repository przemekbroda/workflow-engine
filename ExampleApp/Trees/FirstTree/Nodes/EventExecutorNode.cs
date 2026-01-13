using EventSourcingEngine;

namespace ExampleApp.Trees.FirstTree.Nodes;

public class EventExecutorNode : BaseNodeExecutor<TestState, Event>
{
    public override async Task<Event> ExecuteAsync(Event e, CancellationToken cancellationToken)
    {
        return new Event("ResultFetched", 500);
    }

    protected override void UpdateState(Event e)
    {
        if (e.EventName == "AwaitingResult")
        {
            Cursor.State.AwaitingResult = true;
        }
        else if (e.EventName == "ResultFetched")
        {
            Cursor.State.Balance += (int)e.Payload!;
        }

    }
}