using EventSourcingEngine;
using TestEventSourcingApproach.Trees.FirstTree;
using TestEventSourcingApproach.Trees.FirstTree.Nodes;

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
            new Event("AwaitingExecution", 300),
            new Event("AwaitingResult", 600),
            new Event("AwaitingResult", 600),
            new Event("AwaitingResult", 600),
            new Event("ResultFetched", 600),
        ];

        var stateInitializer = (object? payload) => new TestState
        {
            Balance = (int)payload!
        };
        
        await firstEventSourceTree.ExecuteTree(events, stateInitializer, cancellationToken);
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();