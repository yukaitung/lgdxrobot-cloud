using LGDXRobot2Cloud.API.DbContexts;
using Microsoft.EntityFrameworkCore;
using LGDXRobot2Cloud.API.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("secrets.json", true, true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration["MySQLConnectionString"];
var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
builder.Services.AddDbContext<LgdxContext>(
    dbContextOptions => dbContextOptions
        .UseMySql(connectionString, serverVersion)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
);

// Navigation Repositories
builder.Services.AddScoped<IApiKeyLocationRepository, ApiKeyLocationRepository>();
builder.Services.AddScoped<IFlowRepository, FlowRepository>();
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
builder.Services.AddScoped<IAutoTaskRepository, AutoTaskRepository>();
builder.Services.AddScoped<ISystemComponentRepository, SystemComponentRepository>();
builder.Services.AddScoped<ITriggerRepository, TriggerRepository>();
builder.Services.AddScoped<IWaypointRepository, WaypointRepository>();

// Robot Repositories
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<INodesCollectionRepository, NodesCollectionRepository>();
builder.Services.AddScoped<IRobotRepository, RobotRepository>();

// Setting Repositories
builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
