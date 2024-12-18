using LGDXRobot2Cloud.Worker.Consumers;
using MassTransit;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("secrets.json", true, true);

builder.Services.AddHttpClient<AutoTaskTriggerConsumer>();

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

var host = builder.Build();
host.Run();
