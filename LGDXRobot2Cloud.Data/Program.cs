using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("secrets.json", true, true);
var connectionString = builder.Configuration["MySQLConnectionString"];
var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
builder.Services.AddDbContext<LgdxContext>(
	dbContextOptions => dbContextOptions
		.UseMySql(connectionString, serverVersion)
		.LogTo(Console.WriteLine, LogLevel.Information)
		.EnableSensitiveDataLogging()
		.EnableDetailedErrors()
);

bool initializeData = bool.Parse(builder.Configuration["initializeData"] ?? "false");
if (initializeData) 
{
	builder.Services.AddIdentity<LgdxUser, LgdxRole>()
  	.AddEntityFrameworkStores<LgdxContext>();
	builder.Services.AddHostedService<InitializeDataRunner>();
}

var app = builder.Build();

app.Run();
