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
	cfg.UseRabbitMq(opt =>
	{
		opt.HostName = builder.Configuration["RabbitMq:HostName"]!;
		opt.Port = int.Parse(builder.Configuration["RabbitMq:Port"]!);
		opt.UserName = builder.Configuration["RabbitMq:UserName"]!;
		opt.Password = builder.Configuration["RabbitMq:Password"]!;
		opt.VirtualHost = builder.Configuration["RabbitMq:VirtualHost"]!;
	})
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
bool retry = false;
await WaitForRabbitMq();
host.Run();

async Task WaitForRabbitMq()
{
	var delay = TimeSpan.FromSeconds(5);
	while (true)
	{
		try
		{
			using var tcp = new System.Net.Sockets.TcpClient();
			await tcp.ConnectAsync(builder.Configuration["RabbitMq:HostName"]!, int.Parse(builder.Configuration["RabbitMq:Port"]!));
			if (retry)
				await Task.Delay(delay); // wait for RabbitMQ to be ready
			return;
		}
		catch
		{
			retry = true;
			Console.WriteLine("Waiting for RabbitMQ...");
			await Task.Delay(delay);
		}
	}
}