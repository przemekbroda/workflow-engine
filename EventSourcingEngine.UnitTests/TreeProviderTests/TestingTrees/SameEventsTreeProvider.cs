using EventSourcingEngine.UnitTests.TreeProviderTests.Nodes;

namespace EventSourcingEngine.UnitTests.TreeProviderTests.TestingTrees;

public class SameEventsTreeProvider : TreeProvider<TreeState, TreeEvent>
{
    public override EventNode<TreeState, TreeEvent> ProvideTree()
    {
        return new EventNode<TreeState, TreeEvent>
        {
            HandlesEvents = [typeof(TreeEvent.Event1)],
            Executor = typeof(Node1),
            ProducesEvents = [typeof(TreeEvent.Event2), typeof(TreeEvent.Event3)],
            NextExecutors =
            [
                new EventNode<TreeState, TreeEvent>
                {
                    HandlesEvents = [typeof(TreeEvent.Event2)],
                    Executor = typeof(Node2),
                    ProducesEvents = [typeof(TreeEvent.Event5)],
                    NextExecutors =
                    [
                        new EventNode<TreeState, TreeEvent>
                        {
                            HandlesEvents = [typeof(TreeEvent.Event5)],
                            Executor = typeof(Node3),
                            ProducesEvents = [typeof(TreeEvent.Event6)],
                            NextExecutors = []
                        }
                    ]
                },
                new EventNode<TreeState, TreeEvent>
                {
                    HandlesEvents = [typeof(TreeEvent.Event3)],
                    Executor = typeof(Node4),
                    ProducesEvents = [typeof(TreeEvent.Event7), typeof(TreeEvent.Event8), typeof(TreeEvent.Event9)],
                    NextExecutors =
                    [
                        new EventNode<TreeState, TreeEvent>
                        {
                            HandlesEvents = [typeof(TreeEvent.Event7), typeof(TreeEvent.Event8)],
                            Executor = typeof(Node5),
                            ProducesEvents = [typeof(TreeEvent.Event10), typeof(TreeEvent.Event11)],
                            NextExecutors = []
                        },
                        new EventNode<TreeState, TreeEvent>
                        {
                            HandlesEvents = [typeof(TreeEvent.Event8), typeof(TreeEvent.Event9)],
                            Executor = typeof(Node6),
                            ProducesEvents = [typeof(TreeEvent.Event12)],
                            NextExecutors = []
                        }
                    ]
                },
            ]
        };
    }
}