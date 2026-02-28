using EventSourcingEngine.Exceptions;
using EventSourcingEngine.UnitTests.TreeProviderTests.Nodes;
using EventSourcingEngine.UnitTests.TreeProviderTests.TestingTrees;

namespace EventSourcingEngine.UnitTests.TreeProviderTests;

public class TreeProviderTests
{
    [Fact]
    public void TreeProviderConstructor_ShouldThrowEventSourcingEngineTreeValidationException_WhenTreeDoesNotImplementINodeExecutor()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(new object());

        // Act & Assert
        var exception = Assert.Throws<EventSourcingEngineTreeValidationException>(() =>
            new InvalidExecutorTreeProvider());

        Assert.Equal("Executor must implement INodeExecutor", exception.Message);
    }

    [Fact]
    public void TreeProviderConstructor_ShouldThrowEventSourcingEngineTreeValidationException_WhenNodesOnSameLevelHandleSameEvent()
    {
        // Act & Assert
        var exception = Assert.Throws<EventSourcingEngineTreeValidationException>(() =>
            new SameEventsTreeProvider());
        
        Assert.Equal($"Child node handles same event ({typeof(TreeEvent.Event8)}) as other node with the same parent node", exception.Message);
    }

    [Fact]
    public void TreeProviderConstructor_ShouldThrowEventSourcingEngineTreeValidationException_WhenNextExecutorHandlesEventThatIsNotProducedByParent()
    {
        // Act & Assert
        var exception = Assert.Throws<EventSourcingEngineTreeValidationException>(() =>
            new NotValidProducedEventTreeProvider());
        
        Assert.Equal($"Node with an executor {nameof(Node6)} handles event {typeof(TreeEvent.WeirdEvent)} that is not produced by parent node with an executor {nameof(Node4)} or by itself", exception.Message);
    }

    [Fact]
    public void TreeProviderConstructor_ShouldThrowEventSourcingEngineTreeValidationException_WhenHandlesEventsSetIsEmpty()
    {
        // Act & Assert
        var exception = Assert.Throws<EventSourcingEngineTreeValidationException>(() =>
            new EmptyHandlesEventsTreeProvider());
        
        Assert.Equal("Node must handle at least one event", exception.Message);
    }
    
    [Fact]
    public void TreeProviderConstructor_ShouldThrowEventSourcingEngineTreeValidationException_WhenProducesEventsSetIsEmpty()
    {
        // Act & Assert
        var exception = Assert.Throws<EventSourcingEngineTreeValidationException>(() =>
            new EmptyProducesEventsTreeProvider());
        
        Assert.Equal("Node must produce at least one event", exception.Message);
    }

    [Fact]
    public void TreeProviderConstructor_ShouldNotThrow_WhenTreeIsValid()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(new object());
        
        // Act
        _ = new ValidTreeProvider();
    }
}
