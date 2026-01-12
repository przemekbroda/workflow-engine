using System.Text.Json;
using System.Text.Json.Serialization;
using ExampleApp.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ExampleApp.Postgres;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ProcessRequest> ProcessRequests { get; set; }
    public DbSet<ProcessRequestEvent> ProcessRequestEvents { get; set; }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessRequestEvent>()
            .Property(e => e.ProcessRequestEventPayload)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions),
                v => JsonSerializer.Deserialize<ProcessRequestEventPayload>(v, JsonSerializerOptions), 
                ValueComparer.CreateDefault<ProcessRequestEventPayload>(true));

        modelBuilder.Entity<ProcessRequest>()
            .HasIndex(e => e.LastModifiedAt);
        
        
    }
}