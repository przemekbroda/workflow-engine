using EventSourcingEngine;
using TestEventSourcingApproach.Trees.FirstTree;
using TestEventSourcingApproach.Trees.FirstTree.Nodes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<FirstEventSourceTree>();
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

app.MapGet("/execute-tree",  async (FirstEventSourceTree firstEventSourceTree, CancellationToken cancellationToken) =>
    {
        firstEventSourceTree.SetupCursor([
            new Event("AwaitingExecution", 300),
            new Event("AwaitingResult", 600),
            new Event("AwaitingResult", 600),
            new Event("AwaitingResult", 600),
            new Event("ResultFetched", 600),
        ]);
        
        await firstEventSourceTree.ExecuteTree(payload => new TestState
        {
            Balance = (int)payload!
        }, cancellationToken);
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();