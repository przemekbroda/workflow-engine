using EventSourcingEngine.UnitTests.SimpleTreeTests.Tree;

namespace EventSourcingEngine.UnitTests.SimpleTreeTests.Nodes;

public class ResultSaverNode : BaseNodeExecutor<SimpleTreeState, SimpleTreeEvent>
{
    public override async Task<SimpleTreeEvent> ExecuteAsync(SimpleTreeEvent @event, CancellationToken cancellationToken)
    {
        return @event switch
        {
            SimpleTreeEvent.ResultFetched => new SimpleTreeEvent.ResultSaveError("Result save error"),
            SimpleTreeEvent.ResultSaveError => new SimpleTreeEvent.ResultSaved(),
            _ => throw new Exception($"unhandled event: {@event.GetType().Name}")
        };
    }

    protected override SimpleTreeState UpdateState(SimpleTreeEvent @event)
    {
        return @event switch
        {
            SimpleTreeEvent.ResultSaved => Cursor.State with
            {
                SaveResult = new SaveResult(null, true)
            },
            SimpleTreeEvent.ResultSaveError error => Cursor.State with
            {
                SaveResult = new SaveResult(error.ErrorMessage, false)
            },
            _ => throw new Exception($"unhandled event: {@event.GetType().Name}")
        };
    }
}