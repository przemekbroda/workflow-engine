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
            new InvalidExecutorTreeProvider(serviceProviderMock.Object));

        Assert.Equal("Executor must implement INodeExecutor", exception.Message);
    }
    
    [Fact]
    public void TreeProviderConstructor_ShouldThrowEventSourcingEngineTreeValidationException_WhenExecutorIsNotProvidedToDI()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(It.Is<Type>(t => t == typeof(Node1)))).Returns(new object());
        serviceProviderMock.Setup(sp => sp.GetService(It.Is<Type>(t => t == typeof(Node2)))).Returns(new object());
        serviceProviderMock.Setup(sp => sp.GetService(It.Is<Type>(t => t == typeof(Node3)))).Returns(new object());
        serviceProviderMock.Setup(sp => sp.GetService(It.Is<Type>(t => t == typeof(Node4)))).Returns(new object());
        serviceProviderMock.Setup(sp => sp.GetService(It.Is<Type>(t => t == typeof(Node5)))).Returns(new object());
        
        // Act & Assert
        var exception = Assert.Throws<EventSourcingEngineTreeValidationException>(() =>
            new TestingTreeProvider(serviceProviderMock.Object));
        
        Assert.Equal($"Executor {nameof(Node6)} has not been provided to DI", exception.Message);
    }

    [Fact]
    public void TreeProviderConstructor_ShouldThrowEventSourcingEngineTreeValidationException_WhenNodesOnSameLevelHandleSameEvent()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(new object());
        
        // Act & Assert
        var exception = Assert.Throws<EventSourcingEngineTreeValidationException>(() =>
            new SameEventsTreeProvider(serviceProviderMock.Object));
        
        Assert.Equal("Child nodes handles same event (Event42) as other node with the same parent node", exception.Message);
    }

    [Fact]
    public void TreeProviderConstructor_ShouldThrowEventSourcingEngineTreeValidationException_WhenNextExecutorHandlesEventThatIsNotProducedByParent()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(new object());
        
        // Act & Assert
        var exception = Assert.Throws<EventSourcingEngineTreeValidationException>(() =>
            new NotValidProducedEventTreeProvider(serviceProviderMock.Object));
        
        Assert.Equal($"Node with an executor {nameof(Node6)} handles event WeirdEvent that is not produced by parent node with an executor {nameof(Node4)} or by itself", exception.Message);
    }

    [Fact]
    public void TreeProviderConstructor_ShouldThrowEventSourcingEngineTreeValidationException_WhenHandlesEventsSetIsEmpty()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(new object());
        
        // Act & Assert
        var exception = Assert.Throws<EventSourcingEngineTreeValidationException>(() =>
            new EmptyHandlesEventsTreeProvider(serviceProviderMock.Object));
        
        Assert.Equal("Node must handle at least one event", exception.Message);
    }
    
    [Fact]
    public void TreeProviderConstructor_ShouldThrowEventSourcingEngineTreeValidationException_WhenProducesEventsSetIsEmpty()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(new object());
        
        // Act & Assert
        var exception = Assert.Throws<EventSourcingEngineTreeValidationException>(() =>
            new EmptyProducesEventsTreeProvider(serviceProviderMock.Object));
        
        Assert.Equal("Node must produce at least one event", exception.Message);
    }

    [Fact]
    public void TreeProviderConstructor_ShouldNotThrow_WhenTreeIsValid()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(new object());
        
        // Act
        _ = new ValidTreeProvider(serviceProviderMock.Object);
    }
}
