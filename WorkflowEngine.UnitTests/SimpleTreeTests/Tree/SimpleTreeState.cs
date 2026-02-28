namespace EventSourcingEngine.UnitTests.SimpleTreeTests.Tree;

public record SimpleTreeState
{
    public int Balance { get; init; }
    public bool AwaitingResult { get; init; }
    public int Attempt { get; init; } = 0;
    public SaveResult? SaveResult { get; set; }
}

public record SaveResult(string? ErrorMessage, bool Success);