using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Database Connections
builder.Services.AddDbContext<LgdxContext>(cfg => 
  cfg.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
	.LogTo(Console.WriteLine, LogLevel.Information)
	.EnableSensitiveDataLogging()
	.EnableDetailedErrors()
);
builder.Services.AddIdentity<LgdxUser, LgdxRole>()
  .AddEntityFrameworkStores<LgdxContext>();
builder.Services.AddDbContext<LgdxLogsContext>(cfg =>
	cfg.UseNpgsql(builder.Configuration.GetConnectionString("Activity"))
	.LogTo(Console.WriteLine, LogLevel.Information)
	.EnableSensitiveDataLogging()
	.EnableDetailedErrors()
);

var app = builder.Build();
// Initialize Data
if (bool.Parse(builder.Configuration["initialiseData"] ?? "false"))
{
	// Check configuration
	if (string.IsNullOrEmpty(builder.Configuration["email"]) ||
		string.IsNullOrEmpty(builder.Configuration["fullName"]) ||
		string.IsNullOrEmpty(builder.Configuration["userName"]) ||
		string.IsNullOrEmpty(builder.Configuration["password"]))
	{
		Console.WriteLine("Please provide the following details for the first user: email, full name, username, and password.");
		Environment.Exit(1);
	}

	using var scope = app.Services.CreateScope();
	InitialiseDataRunner initialiseDataRunner = new(scope.ServiceProvider.GetRequiredService<LgdxContext>()!, scope.ServiceProvider.GetRequiredService<LgdxLogsContext>()!, scope.ServiceProvider.GetRequiredService<UserManager<LgdxUser>>()!, builder.Configuration);
	await initialiseDataRunner.StartAsync(CancellationToken.None);
}