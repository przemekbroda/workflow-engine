using EventSourcingEngine.UnitTests.SimpleTreeTests.Nodes;
using EventSourcingEngine.UnitTests.SimpleTreeTests.Tree;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingEngine.UnitTests.SimpleTreeTests;

public class SimpleTreeTests
{
    private readonly IEventSourceTree<SimpleTreeState, SimpleTreeEvent, SimpleTreeProvider> _tree;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public SimpleTreeTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<FirstEventExecutorNode>();
        serviceCollection.AddTransient<ResultSaverNode>();
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