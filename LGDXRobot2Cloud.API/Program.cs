using LGDXRobot2Cloud.API.DbContexts;
using Microsoft.EntityFrameworkCore;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.API.Services;
using Microsoft.AspNetCore.Authentication.Certificate;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("secrets.json", true, true);
builder.WebHost.UseUrls("https://localhost:5162");
builder.WebHost.ConfigureKestrel(cfg =>
{
  cfg.ConfigureHttpsDefaults(ctx => ctx.ClientCertificateMode = ClientCertificateMode.AllowCertificate);
});

builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(cfg =>
    {
			cfg.AllowedCertificateTypes = CertificateTypes.Chained;
			cfg.RevocationMode = X509RevocationMode.NoCheck;
			cfg.Events = new CertificateAuthenticationEvents()
			{
				OnCertificateValidated = ctx =>
				{
					if (ctx.ClientCertificate.Issuer == builder.Configuration["LgdxRobot2:GrpcCertificateIssuer"])
					{
						// The subject must have CN=
						if (ctx.ClientCertificate.Subject.Length < 4)
							ctx.Fail("Incorrect Subject.");
						var claims = new [] {
							new Claim(ClaimTypes.NameIdentifier, ctx.ClientCertificate.Subject[3..])
						};
						ctx.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, ctx.Scheme.Name));
						ctx.Success();
					}
					else
					{
						ctx.Fail("Invalid Issuer.");
					}
					return Task.CompletedTask;
				}
			};
    });

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

// Custom Services
builder.Services.AddScoped<IAutoTaskSchedulerService, AutoTaskSchedulerService>();
builder.Services.AddSingleton<IRobotDataService, RobotDataService>();

// Navigation Repositories
builder.Services.AddScoped<IApiKeyLocationRepository, ApiKeyLocationRepository>();
builder.Services.AddScoped<IFlowRepository, FlowRepository>();
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
builder.Services.AddScoped<IAutoTaskRepository, AutoTaskRepository>();
builder.Services.AddScoped<IAutoTaskDetailRepository, AutoTaskDetailRepository>();
builder.Services.AddScoped<ISystemComponentRepository, SystemComponentRepository>();
builder.Services.AddScoped<ITriggerRepository, TriggerRepository>();
builder.Services.AddScoped<IWaypointRepository, WaypointRepository>();

// Robot Repositories
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<INodesCollectionRepository, NodesCollectionRepository>();
builder.Services.AddScoped<IRobotRepository, RobotRepository>();
builder.Services.AddScoped<IRobotSystemInfoRepository, RobotSystemInfoRepository>();

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
