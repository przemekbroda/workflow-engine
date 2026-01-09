namespace ExampleApp.Trees.FirstTree;

public record struct TestState
{
    public int Balance { get; set; }
    public bool AwaitingResult { get; set; }
}