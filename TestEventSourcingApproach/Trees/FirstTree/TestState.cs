namespace TestEventSourcingApproach.Trees.FirstTree;

public record TestState
{
    public int Balance { get; set; }
    public bool AwaitingResult { get; set; }
}