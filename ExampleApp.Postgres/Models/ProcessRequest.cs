using System.ComponentModel.DataAnnotations.Schema;
using ExampleApp.Postgres.Trees.FirstTree;

namespace ExampleApp.Postgres.Models;

public class ProcessRequest
{
    public required Guid Id { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    [Column(TypeName = "jsonb")] 
    public TestState? TestStateSnapshot { get; set; }


    public IList<ProcessRequestEvent>? ProcessRequestEvents { get; set; }
}