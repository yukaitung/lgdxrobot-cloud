using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Services;
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

bool runSql = bool.Parse(builder.Configuration["runSql"] ?? "false");
if (runSql)
    builder.Services.AddHostedService<SqlScriptRunner>();

var app = builder.Build();

app.Run();
