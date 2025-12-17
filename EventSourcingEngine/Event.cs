namespace EventSourcingEngine;

public record Event(string EventName, object? Payload)
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}