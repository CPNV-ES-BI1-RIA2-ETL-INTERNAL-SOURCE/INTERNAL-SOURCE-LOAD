using INTERNAL_SOURCE_LOAD;
using INTERNAL_SOURCE_LOAD.Models;
using INTERNAL_SOURCE_LOAD.Services;
using System;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings")); 
builder.Services.AddTransient(typeof(IJsonToModelTransformer<>), typeof(JsonToModelTransformer<>));
// Try to get the environment variable value
foreach (var env in Environment.GetEnvironmentVariables())
{
    Console.WriteLine(env);
}

    string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings");
if (connectionString == null)
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

builder.Services.AddSingleton<IDatabaseExecutor, MariaDbExecutor>(sp =>
    new MariaDbExecutor(connectionString));


var app = builder.Build();
app.UseSwagger();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();