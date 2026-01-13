using EventSourcingEngine;
using ExampleApp.Trees.FirstTree;
using ExampleApp.Trees.FirstTree.Nodes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.RegisterTree<TestState, Event, FirstTreeProvider>();
builder.Services.AddTransient<EventExecutorNode>();
builder.Services.AddTransient<ResultSaverNode>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/execute-tree",  async (IEventSourceTree<TestState, Event> firstEventSourceTree, CancellationToken cancellationToken) =>
    {
        List<Event> events =
        [
            new("AwaitingExecution", 300),
            new("AwaitingResult", 1),
            new("AwaitingResult", 2),
            new("AwaitingResult", 3),
            new("ResultFetched", 600),
        ];
        
        // List<Event> events =
        // [
        //     new("AwaitingExecution", 300),
        //     new("AwaitingResult", 1),
        //     new("AwaitingResult", 2),
        //     new("AwaitingResult", 3),
        // ];
        //
        // List<Event> events =
        // [
        //     new("AwaitingExecution", 300),
        //     new("ResultFetched", 600),
        // ];
        //
        // List<Event> events =
        // [
        //     new("AwaitingExecution", 300),
        // ];

        events.Reverse();

        var stateInitializer = (object? payload) => new TestState
        {
            Balance = (int)payload!
        };
        
        var finishedWithEvent = await firstEventSourceTree.ExecuteTree(events, stateInitializer, cancellationToken);
        
        Console.WriteLine($"Finished with event {finishedWithEvent}");
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();