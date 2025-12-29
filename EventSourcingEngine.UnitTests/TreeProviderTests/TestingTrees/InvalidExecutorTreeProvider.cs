using EventSourcingEngine.UnitTests.TreeProviderTests.Nodes;

namespace EventSourcingEngine.UnitTests.TreeProviderTests.TestingTrees;

public class InvalidExecutorTreeProvider(IServiceProvider serviceProvider) : TreeProvider<TreeState, TreeEvent>(serviceProvider)
{
    public override EventNode<TreeState, TreeEvent> ProvideTree()
    {
        return new EventNode<TreeState, TreeEvent>
        {
            HandlesEvents = ["Event1"],
            Executor = typeof(Node1),
            ProducesEvents = ["Event21", "Event22"],
            NextExecutors = [
                new EventNode<TreeState, TreeEvent>
                {
                    HandlesEvents = ["Event21"],
                    Executor = typeof(Node2),
                    ProducesEvents = ["Event31"],
                    NextExecutors = [
                        new EventNode<TreeState, TreeEvent>
                        {
                            HandlesEvents = ["Event31"],
                            Executor = typeof(Node3),
                            ProducesEvents = ["Node3Event"],
                            NextExecutors = []
                        }
                    ]
                },
                new EventNode<TreeState, TreeEvent>
                {
                    HandlesEvents = ["Event22"],
                    Executor = typeof(Node4),
                    ProducesEvents = ["Event41", "Event42"],
                    NextExecutors = [
                        new EventNode<TreeState, TreeEvent>
                        {
                            HandlesEvents = ["Event41"],
                            Executor = typeof(Node5),
                            ProducesEvents = ["Event51", "Event52"],
                            NextExecutors = []
                        },
                        new EventNode<TreeState, TreeEvent>
                        {
                            HandlesEvents = ["Event42"],
                            Executor = typeof(NotImplementingNode),
                            ProducesEvents = ["Event61"],
                            NextExecutors = []
                        }
                    ]
                },
            ]
        };
    }
}