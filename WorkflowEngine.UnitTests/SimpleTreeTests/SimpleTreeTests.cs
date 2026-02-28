using EventSourcingEngine.UnitTests.SimpleTreeTests.Nodes;
using EventSourcingEngine.UnitTests.SimpleTreeTests.Tree;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingEngine.UnitTests.SimpleTreeTests;

public class SimpleTreeTests
{
    private readonly IEventSourceTree<SimpleTreeState, SimpleTreeEvent, SimpleTreeProvider> _tree;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Mock<FirstEventExecutorNode> _firstEventExecutorNodeMock = new();
    private readonly Mock<ResultSaverNode> _resultSaverNodeMock = new();
    
    public SimpleTreeTests()
    {
        var serviceCollection = new ServiceCollection();
        PrepareNodesMocks();
        serviceCollection.AddTransient<FirstEventExecutorNode>(_ => _firstEventExecutorNodeMock.Object);
        serviceCollection.AddTransient<ResultSaverNode>(_ => _resultSaverNodeMock.Object);
        serviceCollection.AddLogging();
        serviceCollection.RegisterTree<SimpleTreeState, SimpleTreeEvent, SimpleTreeProvider>();
        
        _tree = serviceCollection.BuildServiceProvider().GetRequiredService<IEventSourceTree<SimpleTreeState, SimpleTreeEvent, SimpleTreeProvider>>();
    }

    public static IEnumerable<object[]> AwaitingResultsWithAttempts()
    {
        yield return [Enumerable.Repeat<SimpleTreeEvent>(new SimpleTreeEvent.AwaitingResult(), 1).ToList(), 1];
        yield return [Enumerable.Repeat<SimpleTreeEvent>(new SimpleTreeEvent.AwaitingResult(),2).ToList(), 2];
        yield return [Enumerable.Repeat<SimpleTreeEvent>(new SimpleTreeEvent.AwaitingResult(), 3).ToList(), 3];
        yield return [Enumerable.Repeat<SimpleTreeEvent>(new SimpleTreeEvent.AwaitingResult(), 5).ToList(), 5];
        yield return [Enumerable.Repeat<SimpleTreeEvent>(new SimpleTreeEvent.AwaitingResult(), 8).ToList(), 8];
        yield return [Enumerable.Repeat<SimpleTreeEvent>(new SimpleTreeEvent.AwaitingResult(), 10).ToList(), 10];
        
    }

    [Theory]
    [MemberData(nameof(AwaitingResultsWithAttempts))]
    public async Task ExecuteTree_MultipleAwaitingResultEvents_ShouldCalculateStateWithMultipleAttempts(List<SimpleTreeEvent> awaitingResultEvents, int expectedAttempts)
    {
        List<SimpleTreeEvent> events =
        [
            new SimpleTreeEvent.AwaitingExecution(1200),
            ..awaitingResultEvents
        ];
        events.Reverse();

        var result = await _tree.ExecuteTree(events, InitializeState, _cancellationTokenSource.Token);
        var producedEvent = result.ProducedEvent;
        var producedState = result.ProducedState;
        
        Assert.IsType<SimpleTreeEvent.ResultSaveError>(producedEvent);
        Assert.Equal(1700, producedState.Balance);
        Assert.Equal(expectedAttempts, producedState.Attempt);
        Assert.Equal("Result save error", producedState.SaveResult!.ErrorMessage);
        _firstEventExecutorNodeMock.Verify(x => x.ExecuteAsync(It.IsAny<SimpleTreeEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _firstEventExecutorNodeMock.Verify(x => x.TryUpdateState(It.IsAny<SimpleTreeEvent>()), Times.Exactly(expectedAttempts + 1));
        _resultSaverNodeMock.Verify(x => x.ExecuteAsync(It.IsAny<SimpleTreeEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _resultSaverNodeMock.Verify(x => x.TryUpdateState(It.IsAny<SimpleTreeEvent>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteTree_WithOnlyAwaitingExecutionEvent_ShouldNotHaveAttemptsAndReturnResultSaveError()
    {
        List<SimpleTreeEvent> events =
        [
            new SimpleTreeEvent.AwaitingExecution(1200),
        ];

        var result = await _tree.ExecuteTree(events, InitializeState, _cancellationTokenSource.Token);
        var producedEvent = result.ProducedEvent;
        var producedState = result.ProducedState;
        
        Assert.IsType<SimpleTreeEvent.ResultSaveError>(producedEvent);
        Assert.Equal(1700, producedState.Balance);
        Assert.Equal(0, producedState.Attempt);
        Assert.Equal("Result save error", producedState.SaveResult!.ErrorMessage);
        Assert.False(producedState.SaveResult!.Success);
        
        _firstEventExecutorNodeMock.Verify(x => x.ExecuteAsync(It.IsAny<SimpleTreeEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _firstEventExecutorNodeMock.Verify(x => x.TryUpdateState(It.IsAny<SimpleTreeEvent>()), Times.Once);
        _resultSaverNodeMock.Verify(x => x.ExecuteAsync(It.IsAny<SimpleTreeEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _resultSaverNodeMock.Verify(x => x.TryUpdateState(It.IsAny<SimpleTreeEvent>()), Times.Once);
    }
    
    [Theory]
    [MemberData(nameof(AwaitingResultsWithAttempts))]
    public async Task ExecuteTree_HasAwaitingExecutionsAndResultFetchedAndResultSaveError_ShouldNotHaveAttemptsAndReturnResultSavedEvent(List<SimpleTreeEvent> awaitingResultEvents, int expectedAttempts)
    {
        List<SimpleTreeEvent> events =
        [
            new SimpleTreeEvent.AwaitingExecution(1200),
            ..awaitingResultEvents,
            new SimpleTreeEvent.ResultFetched(300),
            new SimpleTreeEvent.ResultSaveError("error message")
        ];
        events.Reverse();

        var result = await _tree.ExecuteTree(events, InitializeState, _cancellationTokenSource.Token);
        var producedEvent = result.ProducedEvent;
        var producedState = result.ProducedState;
        
        Assert.IsType<SimpleTreeEvent.ResultSaved>(producedEvent);
        Assert.Equal(1500, producedState.Balance);
        Assert.Equal(expectedAttempts, producedState.Attempt);
        Assert.NotNull(producedState.SaveResult);
        Assert.Null(producedState.SaveResult.ErrorMessage);
        Assert.True(producedState.SaveResult.Success);
        
        _firstEventExecutorNodeMock.Verify(x => x.ExecuteAsync(It.IsAny<SimpleTreeEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _firstEventExecutorNodeMock.Verify(x => x.TryUpdateState(It.IsAny<SimpleTreeEvent>()), Times.Exactly(expectedAttempts + 1));
        _resultSaverNodeMock.Verify(x => x.ExecuteAsync(It.IsAny<SimpleTreeEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _resultSaverNodeMock.Verify(x => x.TryUpdateState(It.IsAny<SimpleTreeEvent>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteTree_HasResultFetched_ShouldReturnResultSaveErrorEvent()
    {
        List<SimpleTreeEvent> events =
        [
            new SimpleTreeEvent.AwaitingExecution(1200),
            new SimpleTreeEvent.ResultFetched(400),
        ];
        events.Reverse();

        var result = await _tree.ExecuteTree(events, InitializeState, _cancellationTokenSource.Token);
        var producedEvent = result.ProducedEvent;
        var producedState = result.ProducedState;
        
        Assert.IsType<SimpleTreeEvent.ResultSaveError>(producedEvent);
        Assert.Equal(1600, producedState.Balance);
        Assert.Equal(0, producedState.Attempt);
        Assert.NotNull(producedState.SaveResult);
        Assert.Equal("Result save error", producedState.SaveResult.ErrorMessage);
        Assert.False(producedState.SaveResult.Success);
        
        _firstEventExecutorNodeMock.Verify(x => x.ExecuteAsync(It.IsAny<SimpleTreeEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _firstEventExecutorNodeMock.Verify(x => x.TryUpdateState(It.IsAny<SimpleTreeEvent>()), Times.Once);
        _resultSaverNodeMock.Verify(x => x.ExecuteAsync(It.IsAny<SimpleTreeEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _resultSaverNodeMock.Verify(x => x.TryUpdateState(It.IsAny<SimpleTreeEvent>()), Times.Once);
    }

    private void PrepareNodesMocks()
    {
        _firstEventExecutorNodeMock
            .Setup(x => x.ExecuteAsync(It.IsAny<SimpleTreeEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimpleTreeEvent.ResultFetched(500));

        _firstEventExecutorNodeMock
            .Setup(x => x.TryUpdateState(It.IsAny<SimpleTreeEvent>()))
            .Returns<SimpleTreeEvent>(e => e switch
            {
                SimpleTreeEvent.AwaitingResult => _firstEventExecutorNodeMock.Object.Cursor.State with
                {
                    AwaitingResult = true,
                    Attempt = _firstEventExecutorNodeMock.Object.Cursor.State.Attempt + 1
                },
                SimpleTreeEvent.ResultFetched resultFetched => _firstEventExecutorNodeMock.Object.Cursor.State with
                {
                    AwaitingResult = false, Balance = _firstEventExecutorNodeMock.Object.Cursor.State.Balance + resultFetched.Amount
                },
                _ => _firstEventExecutorNodeMock.Object.Cursor.State
            });

        _resultSaverNodeMock
            .Setup(x => x.ExecuteAsync(It.IsAny<SimpleTreeEvent>(), It.IsAny<CancellationToken>()))
            .Returns<SimpleTreeEvent, CancellationToken>((@event, _) => Task.FromResult(@event switch
            {
                SimpleTreeEvent.ResultFetched => (SimpleTreeEvent)new SimpleTreeEvent.ResultSaveError("Result save error"),
                SimpleTreeEvent.ResultSaveError => new SimpleTreeEvent.ResultSaved(),
                _ => throw new Exception($"unhandled event: {@event.GetType().Name}")
            }));

        _resultSaverNodeMock
            .Setup(x => x.TryUpdateState(It.IsAny<SimpleTreeEvent>()))
            .Returns<SimpleTreeEvent>(@event => @event switch
            {
                SimpleTreeEvent.ResultSaved => _resultSaverNodeMock.Object.Cursor.State with
                {
                    SaveResult = new SaveResult(null, true)
                },
                SimpleTreeEvent.ResultSaveError error => _resultSaverNodeMock.Object.Cursor.State with
                {
                    SaveResult = new SaveResult(error.ErrorMessage, false)
                },
                _ => throw new Exception($"unhandled event: {@event.GetType().Name}")
            });
    }
    
    private static SimpleTreeState InitializeState(SimpleTreeEvent @event)
    {
        if (@event is SimpleTreeEvent.AwaitingExecution e)
        {
            return new SimpleTreeState
            {
                Balance = e.Balance
            };
        }
        
        throw new Exception();
    }
}