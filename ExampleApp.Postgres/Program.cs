using System.Data;
using System.Text.Json.Serialization;
using EventSourcingEngine;
using ExampleApp.Postgres;
using ExampleApp.Postgres.Models;
using ExampleApp.Postgres.Trees.FirstTree;
using ExampleApp.Postgres.Trees.FirstTree.Nodes;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.RegisterTree<TestState, FirstTreeEvent, FirstTreeProvider>();
builder.Services.AddScoped<EventExecutorNode>();
builder.Services.AddScoped<ResultSaverNode>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<AppDbContext>(b =>
{
    b.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/process", async (AppDbContext dbContext) =>
    {
        var processRequest = new ProcessRequest
        {
            Id = Guid.CreateVersion7(),
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
            Version = 1,
            ProcessRequestEvents = new List<ProcessRequestEvent>
            {
                new()
                {
                    EventName = "AwaitingExecution",
                    ProcessRequestEventPayload = new AwaitingExecution(1000),
                    Index = 0,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };
        
        dbContext.ProcessRequests.Add(processRequest);
        
        await dbContext.SaveChangesAsync();
        
        return TypedResults.Ok(processRequest.Id);
    })
    .WithName("GetWeatherForecast");

app.MapGet("/process/{id:guid}", async (AppDbContext dbContext, Guid id) =>
{
    var result = await dbContext.ProcessRequests
        .Include(x => x.ProcessRequestEvents)
        .FirstOrDefaultAsync(x => x.Id == id);

    return TypedResults.Ok(result);
});

app.MapPatch("/process/{id:guid}", async (
    Guid id, 
    AppDbContext dbContext, 
    IEventSourceTree<TestState, FirstTreeEvent, FirstTreeProvider> eventSource, 
    CancellationToken cancellationToken) =>
{
    using (var transaction = dbContext.Database.BeginTransaction(IsolationLevel.ReadCommitted))
    {
        try
        {
            var process = await dbContext.ProcessRequests
                .FromSql($"""SELECT * FROM "ProcessRequests" FOR UPDATE SKIP LOCKED""")
//                  .FromSql($"""
//                            SELECT * FROM "ProcessRequests"
//                            WHERE 
//                                "LastModifiedAt" <= {DateTime.UtcNow.AddSeconds(-30)}
//                            ORDER BY "LastModifiedAt" 
//                            LIMIT 1 
//                            FOR UPDATE SKIP LOCKED
//                            """)
                .Include(x => x.ProcessRequestEvents)
                // .FirstOrDefaultAsync();
                .FirstOrDefaultAsync(x => x.Id == id);

            if (process is null)
            {
                return TypedResults.Ok(); 
            }

            var events = (process.ProcessRequestEvents ?? [])
                .Select(dbEvent => dbEvent.ToTreeEvent())
                .OrderByDescending(e => e.Index)
                .ToList();

            var result = await eventSource.ExecuteTree(events, @event =>
            {
                if (@event is not FirstTreeEvent.AwaitingExecution execution)
                {
                    throw new Exception();
                }

                return new TestState
                {
                    Balance = execution.Balance,
                    AwaitingResult = false,
                    ProcessRequestId = process.Id,
                };
            }, cancellationToken);

            Console.WriteLine($"Finished with event {result}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            //we want to save anything that could be stored that was generated in nodes, in the db even when we've got an exception
            await transaction.CommitAsync(CancellationToken.None);
        }
    }
    
    return TypedResults.Ok();
});


app.Run();
