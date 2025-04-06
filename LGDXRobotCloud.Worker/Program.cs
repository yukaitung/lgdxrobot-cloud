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
builder.Services.AddDbContextPool<LgdxContext>(cfg => 
  cfg.UseNpgsql(builder.Configuration["PGSQLConnectionString"])
	.LogTo(Console.WriteLine, LogLevel.Information)
	.EnableSensitiveDataLogging()
	.EnableDetailedErrors()
);
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
builder.Services.AddTransient<HtmlRenderer>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddHttpClient<ITriggerService, TriggerService>();

var host = builder.Build();
host.Run();
