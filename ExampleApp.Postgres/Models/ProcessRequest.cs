namespace ExampleApp.Postgres.Models;

public class ProcessRequest
{
    public long Id { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }

    public IList<ProcessRequestEvent>? ProcessRequestEvents { get; set; }
}