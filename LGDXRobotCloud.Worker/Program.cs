using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Worker.Configurations;
using LGDXRobotCloud.Worker.Services;
using MassTransit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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
builder.Services.AddMassTransit(cfg =>
{
  var entryAssembly = Assembly.GetEntryAssembly();
  cfg.AddConsumers(entryAssembly);

	cfg.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host(builder.Configuration["RabbitMq:Host"], builder.Configuration["RabbitMq:VirtualHost"], h =>
		{
			h.Username(builder.Configuration["RabbitMq:Username"] ?? string.Empty);
			h.Password(builder.Configuration["RabbitMq:Password"] ?? string.Empty);
		});
		cfg.ConfigureEndpoints(context);
	});
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
