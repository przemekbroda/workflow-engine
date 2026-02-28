namespace EventSourcingEngine.UnitTests.SimpleTreeTests.Tree;

public abstract record SimpleTreeEvent
{
    public record AwaitingExecution(int Balance) : SimpleTreeEvent;
    public record AwaitingResult() : SimpleTreeEvent;
    public record ResultFetched(int Amount) : SimpleTreeEvent;
    public record ResultSaveError(string ErrorMessage) : SimpleTreeEvent;
    public record ResultSaved() : SimpleTreeEvent;
}