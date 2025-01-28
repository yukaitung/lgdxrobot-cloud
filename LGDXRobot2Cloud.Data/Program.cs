using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("secrets.json", true, true);
builder.Services.AddDbContextPool<LgdxContext>(cfg => 
  cfg.UseNpgsql(builder.Configuration["PGSQLConnectionString"])
	.LogTo(Console.WriteLine, LogLevel.Information)
	.EnableSensitiveDataLogging()
	.EnableDetailedErrors()
);

bool initializeData = bool.Parse(builder.Configuration["initialiseData"] ?? "false");
if (initializeData) 
{
	builder.Services.AddIdentity<LgdxUser, LgdxRole>()
  	.AddEntityFrameworkStores<LgdxContext>();
	builder.Services.AddHostedService<InitialiseDataRunner>();
}

var app = builder.Build();

app.Run();
