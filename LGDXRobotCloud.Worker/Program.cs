using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Worker.Configurations;
using LGDXRobotCloud.Worker.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

/*
 * Configuration
 */
builder.Services.Configure<EmailConfiguration>(
	builder.Configuration.GetSection("Email")
);
builder.Services.Configure<EmailLinksConfiguration>(
	builder.Configuration.GetSection("EmailLinks")
);

/*
 * Infrastructure
 */
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDbContext<LgdxContext>(cfg =>
		cfg.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
		.LogTo(Console.WriteLine, LogLevel.Information)
		.EnableSensitiveDataLogging()
		.EnableDetailedErrors()
	);
	builder.Services.AddDbContext<LgdxLogsContext>(cfg =>
		cfg.UseNpgsql(builder.Configuration.GetConnectionString("Activity"))
		.LogTo(Console.WriteLine, LogLevel.Information)
		.EnableSensitiveDataLogging()
		.EnableDetailedErrors()
	);
}
else
{
	builder.Services.AddDbContext<LgdxContext>(cfg =>
		cfg.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
	);
	builder.Services.AddDbContext<LgdxLogsContext>(cfg =>
		cfg.UseNpgsql(builder.Configuration.GetConnectionString("Activity"))
	);
}
builder.UseWolverine(cfg =>
{
	cfg.UseRabbitMq(new Uri(builder.Configuration["RabbitMq:ConnectionString"]!))
		.UseListenerConnectionOnly()
		.AutoProvision();
	cfg.ListenToRabbitQueue("activity-logs-queue");
	cfg.ListenToRabbitQueue("email-queue");
	cfg.ListenToRabbitQueue("auto-task-trigger-queue");
});

/*
 * LGDX Depency Injection
 */
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddTransient<HtmlRenderer>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddHttpClient<ITriggerService, TriggerService>();

var host = builder.Build();
host.Run();
