using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
}

[JsonDerivedType(typeof(AwaitingExecution), nameof(AwaitingExecution))]
[JsonDerivedType(typeof(ResultFetched), nameof(ResultFetched))]
public abstract record ProcessRequestEventPayload;
public record AwaitingExecution(int Balance) : ProcessRequestEventPayload;
public record ResultFetched(int Amount) : ProcessRequestEventPayload;