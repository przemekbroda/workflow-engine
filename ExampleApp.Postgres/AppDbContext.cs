using System.Text.Json;
using System.Text.Json.Serialization;
using ExampleApp.Postgres.Models;
using ExampleApp.Postgres.Trees.FirstTree;
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
        modelBuilder.Entity<ProcessRequestEvent>(entity =>
        {
            entity
                .Property(e => e.ProcessRequestEventPayload)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions),
                    v => JsonSerializer.Deserialize<ProcessRequestEventPayload>(v, JsonSerializerOptions),
                    ValueComparer.CreateDefault<ProcessRequestEventPayload>(true));

            entity
                .Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()");
        });

        modelBuilder.Entity<ProcessRequest>(entity =>
        {
            entity
                .HasIndex(e => e.LastModifiedAt);

            entity
                .Property(e => e.TestStateSnapshot)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions),
                    v => JsonSerializer.Deserialize<TestState>(v, JsonSerializerOptions),
                    ValueComparer.CreateDefault<TestState>(true));

            entity
                .Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()");
        });
    }
}