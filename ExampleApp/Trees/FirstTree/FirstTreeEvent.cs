namespace ExampleApp.Trees.FirstTree;

public abstract record FirstTreeEvent
{
    public record AwaitingExecution(int Balance) : FirstTreeEvent;
    public record AwaitingResult(int Attempt) : FirstTreeEvent;
    public record ResultFetched(int Amount) : FirstTreeEvent;
    public record ResultSaveError() : FirstTreeEvent;
    public record ResultSaved() : FirstTreeEvent;
}