namespace ExampleApp.Postgres.Trees.FirstTree;

public record TestState
{
    public int Balance { get; init; }
    public bool AwaitingResult { get; init; }
    public long ProcessRequestId { get; init; }
}