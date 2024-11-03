using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.API.Services;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Constants;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

/*
 * Configuration
 */
builder.Services.Configure<LgdxRobot2Configuration>(
	builder.Configuration.GetSection("LGDXRobot2")
);
builder.Services.Configure<LgdxRobot2SecretConfiguration>(
	builder.Configuration.GetSection("LGDXRobot2Secret")
);

/*
 * Infrastructure
 */
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(cfg =>{
	cfg.SwaggerDoc("v1", new OpenApiInfo { Title = "LGDXRobot2", Version = "v1" });
	cfg.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
		Description = "JWT Authorization header using the Bearer scheme.",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		BearerFormat = "JWT",
    Scheme = "Bearer"
	});
	cfg.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			[]
		}
	});
});
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
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();

/*
 * Authentication
 */
builder.Services.AddIdentity<LgdxUser, LgdxRole>()
  .AddEntityFrameworkStores<LgdxContext>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(cfg =>
	{
		cfg.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["LGDXRobot2Secret:LgdxUserJwtIssuer"],
			ValidAudience = builder.Configuration["LGDXRobot2Secret:LgdxUserJwtIssuer"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["LGDXRobot2Secret:LgdxUserJwtSecret"] ?? string.Empty)),
			ClockSkew = TimeSpan.Zero
		};
	});
builder.Services.AddTransient<RobotClientsCertificateValidationService>();
builder.Services.AddAuthentication(LgdxRobot2AuthenticationSchemes.RobotClientsCertificateScheme)
	.AddCertificate(LgdxRobot2AuthenticationSchemes.RobotClientsCertificateScheme, cfg =>
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
				var validationService = ctx.HttpContext.RequestServices.GetService<RobotClientsCertificateValidationService>();
				if (!await validationService!.ValidateRobotClientsCertificate(ctx.ClientCertificate, Guid.Parse(guid)))
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
builder.Services.AddAuthentication(LgdxRobot2AuthenticationSchemes.RobotClientsJwtScheme)
	.AddJwtBearer(LgdxRobot2AuthenticationSchemes.RobotClientsJwtScheme, cfg =>
	{
		cfg.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["LGDXRobot2Secret:RobotClientsJwtIssuer"],
			ValidAudience = builder.Configuration["LGDXRobot2Secret:RobotClientsJwtIssuer"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["LGDXRobot2Secret:RobotClientsJwtSecret"] ?? string.Empty)),
			ClockSkew = TimeSpan.Zero
		};
	});
builder.Services.AddScoped<IAuthorizationHandler, ValidateLgdxUserAccessHandler>();
builder.Services.AddAuthorizationBuilder()
	.AddPolicy("ValidateLgdxUserAccess", policyBuilder =>
	{
		policyBuilder.RequireAuthenticatedUser();
		policyBuilder.AddRequirements(new ValidateLgdxUserAccessRequirement());
	});

/*
 * LGDX Depency Injection
 */
// Custom Services
builder.Services.AddScoped<IAutoTaskSchedulerService, AutoTaskSchedulerService>();
builder.Services.AddScoped<IOnlineRobotsService, OnlineRobotsService>();
builder.Services.AddScoped<IFlowTriggersService, FlowTriggersService>();
builder.Services.AddHttpClient<IFlowTriggersService, FlowTriggersService>();

// Identity Repositories
builder.Services.AddScoped<ILgdxUsersRepository, LgdxUsersRepository>();
builder.Services.AddScoped<ILgdxRoleRepository, LgdxRoleRepository>();

// Navigation Repositories
builder.Services.AddScoped<IFlowRepository, FlowRepository>();
builder.Services.AddScoped<IFlowDetailRepository, FlowDetailRepository>();
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
builder.Services.AddScoped<IRobotCertificateRepository, RobotCertificateRepository>();

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

app.MapControllerRoute(
	name: "Area",
	pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapGrpcService<RobotClientsService>();

app.Run();
