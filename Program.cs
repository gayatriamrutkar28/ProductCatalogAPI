using System.Net.Mime;
using System.Text.Json;
using Catalog.Repositories;
using Catalog.Settings;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers(options=>
{
    options.SuppressAsyncSuffixInActionNames = false;
});
builder.Services.AddSwaggerGen();


BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>() ?? throw new InvalidOperationException("MongoDbSettings not configured properly.");

builder.Services.AddSingleton<IMongoClient>(serviceprovider=>
{
    return new MongoClient(mongoDbSettings.ConnectionString);
});

//builder.Services.AddSingleton<IItemsRepository, InMemItemsRepository>(); //registering the InMemItemsRepository
builder.Services.AddSingleton<IItemsRepository, MongoDbItemsRepository>(); //registering the MongoDbItemsRepository
builder.Services.AddHealthChecks()
            .AddMongoDb(
                sp => new MongoClient(mongoDbSettings.ConnectionString), 
                name: "mongodb", 
                timeout: TimeSpan.FromSeconds(3),
                tags: new[] {"ready"});


var app = builder.Build();
app.MapControllers();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("ready"),
    ResponseWriter= async(context,report)=>
    {
        var result= JsonSerializer.Serialize(
            new{
                status= report.Status.ToString(),
                checks= report.Entries.Select(entry=>new
                {
                    name= entry.Key,
                    status= entry.Value.Status.ToString(),
                    exception=entry.Value.Exception!=null ? entry.Value.Exception.Message : "none",
                    duration= entry.Value.Duration.ToString()
                })
            }
        );
        context.Response.ContentType = MediaTypeNames.Application.Json;
        await context.Response.WriteAsync(result);
    }
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = (_) => false
});
app.UseSwaggerUI();
app.UseSwagger();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
if (app.Environment.IsDevelopment())
{
     app.UseHttpsRedirection();

}

// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast =  Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast");

app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
