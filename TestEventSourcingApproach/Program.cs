using System.Threading;
using EventSourcingEngine;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestEventSourcingApproach.Trees.FirstTree;
using TestEventSourcingApproach.Trees.FirstTree.Nodes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<FirstEventSourceTree>();
builder.Services.AddTransient<ResultAwaiterNode>();
builder.Services.AddTransient<EventExecutorNode>();
builder.Services.AddTransient<ResultAwaiterNode>();
builder.Services.AddTransient<ResultSaverNode>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/execute-tree",  async (FirstEventSourceTree firstEventSourceTree, CancellationToken cancellationToken) =>
    {
        firstEventSourceTree.SetupCursor([
            new Event("AwaitingExecution", 300),
            new Event("AwaitingResult", null),
            new Event("AwaitingResult", null),
            new Event("AwaitingResult", null),
            new Event("AwaitingResult", null),
            new Event("ResultFetched", 600),
            new Event("ResultSaveError", null),
            new Event("ResultSaveError", null),
        ]);
        
        await firstEventSourceTree.ExecuteTree(payload => new TestState
        {
            Balance = (int)payload!
        }, cancellationToken);
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();