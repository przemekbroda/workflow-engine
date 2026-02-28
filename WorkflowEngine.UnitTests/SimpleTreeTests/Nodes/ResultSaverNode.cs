using EventSourcingEngine.UnitTests.SimpleTreeTests.Tree;

namespace EventSourcingEngine.UnitTests.SimpleTreeTests.Nodes;

// Mocks will replace the implementation of this class, so I don't care what implementation is here. It's best to keep it simple
public class ResultSaverNode : BaseNodeExecutor<SimpleTreeState, SimpleTreeEvent>
{
    public override async Task<SimpleTreeEvent> ExecuteAsync(SimpleTreeEvent @event, CancellationToken cancellationToken)
    {
        return new SimpleTreeEvent.ResultSaveError("Result save error");
    }

    protected override SimpleTreeState UpdateState(SimpleTreeEvent @event)
    {
        return Cursor.State;
    }
}