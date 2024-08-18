using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Constants;
using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.API.Services;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("secrets.json", true, true);
builder.WebHost.ConfigureKestrel(cfg =>
{
  cfg.ConfigureHttpsDefaults(ctx => ctx.ClientCertificateMode = ClientCertificateMode.AllowCertificate);
	cfg.AddServerHeader = false;
});

// Authentication
builder.Services.AddTransient<RobotClientCertificateValidationService>();
builder.Services.AddAuthentication(LgdxRobot2AuthenticationSchemes.RobotClientCertificateScheme)
	.AddCertificate(LgdxRobot2AuthenticationSchemes.RobotClientCertificateScheme, cfg =>
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
builder.Services.AddAuthentication(LgdxRobot2AuthenticationSchemes.RobotClientJwtScheme)
	.AddJwtBearer(LgdxRobot2AuthenticationSchemes.RobotClientJwtScheme, cfg =>
	{
		cfg.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["LGDXRobot2Secret:RobotClientJwtIssuer"],
			ValidAudience = builder.Configuration["LGDXRobot2Secret:RobotClientJwtIssuer"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["LGDXRobot2Secret:RobotClientJwtSecret"] ?? string.Empty)),
			ClockSkew = TimeSpan.Zero
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
builder.Services.Configure<LgdxRobot2SecretConfiguration>(
	builder.Configuration.GetSection("LGDXRobot2Secret")
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
