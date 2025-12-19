using EventSourcingEngine;

namespace TestEventSourcingApproach.Trees.FirstTree.Nodes;

public class EventExecutorNode : BaseNodeExecutor<TestState>
{
    public override async Task<Event> ExecuteAsync(Event e, CancellationToken cancellationToken)
    {
        return new Event("ResultFetched", 500);
    }

    public override async Task UpdateState(Event e, CancellationToken cancellationToken)
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