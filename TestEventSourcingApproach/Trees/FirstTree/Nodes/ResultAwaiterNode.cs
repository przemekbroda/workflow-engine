using EventSourcingEngine;

namespace TestEventSourcingApproach.Trees.FirstTree.Nodes;

public class ResultAwaiterNode : BaseNodeExecutor<TestState>
{
    public override async Task<Event> ExecuteAsync(Event e, CancellationToken cancellationToken)
    {
        return new Event("ResultFetched", 500);
    }

    public override async Task UpdateState(Event e, CancellationToken cancellationToken)
    {
        switch (e.EventName)
        {
            case "AwaitingResultAgain":
                Cursor.State.AwaitingResult = true;
                break;
            case "ResultFetched":
                Cursor.State.AwaitingResult = false;
                Cursor.State.Balance += (int)e.Payload!;
                break;
        }
    }
}