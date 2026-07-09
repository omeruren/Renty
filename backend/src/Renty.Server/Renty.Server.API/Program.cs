using Renty.Server.Infrastructure;
using Renty.Server.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.Run();

