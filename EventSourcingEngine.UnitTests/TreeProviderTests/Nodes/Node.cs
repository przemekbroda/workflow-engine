namespace EventSourcingEngine.UnitTests.TreeProviderTests.Nodes;

public class TestBaseNode : BaseNodeExecutor<TreeState, TreeEvent>
{
    protected override TreeState UpdateState(TreeEvent e, TreeState state)
    {
        throw new NotImplementedException();
    }

    public override Task<TreeEvent> ExecuteAsync(TreeEvent @event, TreeState treeState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class Node1 : TestBaseNode;
public class Node2 : TestBaseNode;
public class Node3 : TestBaseNode;
public class Node4 : TestBaseNode;
public class Node5 : TestBaseNode;
public class Node6 : TestBaseNode;
public class Node7 : TestBaseNode;
public class Node8 : TestBaseNode;
public class Node9 : TestBaseNode;
public class Node10 : TestBaseNode;