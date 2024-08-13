using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.API.Services;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("secrets.json", true, true);
builder.WebHost.ConfigureKestrel(cfg =>
{
  cfg.ConfigureHttpsDefaults(ctx => ctx.ClientCertificateMode = ClientCertificateMode.AllowCertificate);
});

// Authentication
builder.Services.AddTransient<RobotClientCertificateValidationService>();
builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
	.AddCertificate(cfg =>
	{
		cfg.AllowedCertificateTypes = CertificateTypes.All;
		cfg.RevocationMode = X509RevocationMode.NoCheck;
		cfg.Events = new CertificateAuthenticationEvents()
		{
			OnCertificateValidated = async ctx =>
			{
				string subject = ctx.ClientCertificate.Subject;
				string guid = subject.Substring(subject.IndexOf("OID.0.9.2342.19200300.100.1.1=") + 30, 36);
				if (guid == string.Empty) 
				{
					ctx.Fail("Robot ID not found.");
					return;
				}
				var validationService = ctx.HttpContext.RequestServices.GetService<RobotClientCertificateValidationService>();
				if (!await validationService!.ValidateRobotClientCertificate(ctx.ClientCertificate, Guid.Parse(guid)))
				{
					ctx.Fail("Invalid certificate / Robot not found.");
					return;
				}
				var claims = new [] {
					new Claim(ClaimTypes.NameIdentifier, guid)
				};
				ctx.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, ctx.Scheme.Name));
				ctx.Success();
			}
		};
	});

// Services
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc(cfg => cfg.EnableDetailedErrors = true);
var connectionString = builder.Configuration["MySQLConnectionString"];
var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
builder.Services.AddDbContext<LgdxContext>(
    dbContextOptions => dbContextOptions
        .UseMySql(connectionString, serverVersion)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
);

builder.Services.Configure<LgdxRobot2Configuration>(
	builder.Configuration.GetSection("LGDXRobot2")
);

// Custom Services
builder.Services.AddScoped<IAutoTaskSchedulerService, AutoTaskSchedulerService>();

// Navigation Repositories
builder.Services.AddScoped<IFlowRepository, FlowRepository>();
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
builder.Services.AddScoped<IAutoTaskRepository, AutoTaskRepository>();
builder.Services.AddScoped<IAutoTaskDetailRepository, AutoTaskDetailRepository>();
builder.Services.AddScoped<ITriggerRepository, TriggerRepository>();
builder.Services.AddScoped<IWaypointRepository, WaypointRepository>();

// Robot Repositories
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<INodesCollectionRepository, NodesCollectionRepository>();
builder.Services.AddScoped<IRobotRepository, RobotRepository>();
builder.Services.AddScoped<IRobotSystemInfoRepository, RobotSystemInfoRepository>();
builder.Services.AddScoped<IRobotChassisInfoRepository, RobotChassisInfoRepository>();

// Setting Repositories
builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<RobotClientService>();

app.Run();
