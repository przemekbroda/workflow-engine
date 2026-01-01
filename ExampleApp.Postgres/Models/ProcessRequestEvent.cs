using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ExampleApp.Postgres.Trees.FirstTree;

namespace ExampleApp.Postgres.Models;

public class ProcessRequestEvent
{
    public long Id { get; set; }
    public required string EventName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Index { get; set; }
    [Column(TypeName = "jsonb")] 
    public ProcessRequestEventPayload? ProcessRequestEventPayload { get; set; }

    public ProcessRequest ProcessRequest { get; set; } = null!;
    public long ProcessRequestId { get; set; }

    public FirstTreeEvent GetTreeEvent()
    {
        return EventName switch
        {
            nameof(FirstTreeEvent.AwaitingExecution) => new FirstTreeEvent.AwaitingExecution(((AwaitingExecution)ProcessRequestEventPayload!).Balance, Index),
            nameof(FirstTreeEvent.AwaitingResult) => new FirstTreeEvent.AwaitingResult(Index),
            nameof(FirstTreeEvent.ResultFetched) => new FirstTreeEvent.ResultFetched(((ResultFetched)ProcessRequestEventPayload!).Amount, Index),
            nameof(FirstTreeEvent.ResultSaveError) => new FirstTreeEvent.ResultSaveError(Index),
            nameof(FirstTreeEvent.ResultSaved) => new FirstTreeEvent.ResultSaved(Index),
            _ => throw new Exception("Unknown event name")
        };
    }
}

[JsonDerivedType(typeof(AwaitingExecution), nameof(AwaitingExecution))]
[JsonDerivedType(typeof(ResultFetched), nameof(ResultFetched))]
public abstract record ProcessRequestEventPayload;
public record AwaitingExecution(int Balance) : ProcessRequestEventPayload;
public record ResultFetched(int Amount) : ProcessRequestEventPayload;