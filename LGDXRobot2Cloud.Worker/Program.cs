using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.Worker.Consumers;
using LGDXRobot2Cloud.Worker.Services;
using MassTransit;
using Microsoft.AspNetCore.Components.Web;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("secrets.json", true, true);

/*
 * Configuration
 */
builder.Services.Configure<EmailConfiguration>(
	builder.Configuration.GetSection("Email")
);

/*
 * Infrastructure
 */
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
builder.Services.AddHttpClient<AutoTaskTriggerConsumer>();

var host = builder.Build();
host.Run();
