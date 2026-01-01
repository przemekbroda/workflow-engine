using ExampleApp.Postgres.Models;

namespace ExampleApp.Postgres.Trees.FirstTree;

public record TestState
{
    public int Balance { get; set; }
    public bool AwaitingResult { get; set; }
    public long ProcessRequestId { get; set; }
}