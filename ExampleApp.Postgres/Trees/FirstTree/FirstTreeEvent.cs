namespace ExampleApp.Postgres.Trees.FirstTree;

public abstract record FirstTreeEvent(int Index)
{
    public record AwaitingExecution(int Balance, int Index) : FirstTreeEvent(Index);
    public record AwaitingResult(int Index) : FirstTreeEvent(Index);
    public record ResultFetched(int Amount, int Index) : FirstTreeEvent(Index);
    public record ResultSaveError(int Index) : FirstTreeEvent(Index);
    public record ResultSaved(int Index) : FirstTreeEvent(Index);
}