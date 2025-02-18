using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContextPool<LgdxContext>(cfg => 
  cfg.UseNpgsql(builder.Configuration["PGSQLConnectionString"])
	.LogTo(Console.WriteLine, LogLevel.Information)
	.EnableSensitiveDataLogging()
	.EnableDetailedErrors()
);

bool initializeData = bool.Parse(builder.Configuration["initialiseData"] ?? "false");
if (initializeData) 
{
	// Check connfig
	if (string.IsNullOrEmpty(builder.Configuration["email"]) ||
		string.IsNullOrEmpty(builder.Configuration["fullName"]) ||
		string.IsNullOrEmpty(builder.Configuration["userName"]) ||
		string.IsNullOrEmpty(builder.Configuration["password"]))
	{
		Console.WriteLine("Please provide the following configurations for the first user: Email, Full Name, userName, password");
		Environment.Exit(1);
	}

	builder.Services.AddIdentity<LgdxUser, LgdxRole>()
  	.AddEntityFrameworkStores<LgdxContext>();
	builder.Services.AddHostedService<InitialiseDataRunner>();
}

var app = builder.Build();
app.Run();
