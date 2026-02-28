using EventSourcingEngine.UnitTests.SimpleTreeTests.Tree;

namespace EventSourcingEngine.UnitTests.SimpleTreeTests.Nodes;

// Mocks will replace the implementation of this class, so I don't care what implementation is here. It's best to keep it simple
public class FirstEventExecutorNode : BaseNodeExecutor<SimpleTreeState, SimpleTreeEvent>
{
    public override async Task<SimpleTreeEvent> ExecuteAsync(SimpleTreeEvent @event, CancellationToken cancellationToken)
    {
        return new SimpleTreeEvent.ResultFetched(500);
    }

    protected override SimpleTreeState UpdateState(SimpleTreeEvent e)
    {
        return Cursor.State;
    }
}