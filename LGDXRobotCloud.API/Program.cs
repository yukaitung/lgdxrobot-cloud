using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Middleware;
using LGDXRobotCloud.API.Services;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Services.Identity;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Utilities.Constants;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(cfg =>
{
  cfg.ConfigureHttpsDefaults(ctx => ctx.ClientCertificateMode = ClientCertificateMode.AllowCertificate);
	cfg.AddServerHeader = false;
});

/*
 * Configuration
 */
builder.Services.Configure<LgdxRobotCloudConfiguration>(
	builder.Configuration.GetSection("LGDXRobotCloud")
);
builder.Services.Configure<LgdxRobotCloudSecretConfiguration>(
	builder.Configuration.GetSection("LGDXRobotCloudSecret")
);

/*
 * Infrastructure
 */
builder.Services.AddMassTransit(cfg =>
{
	cfg.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host(builder.Configuration["RabbitMq:Host"], builder.Configuration["RabbitMq:VirtualHost"], h =>
		{
			h.Username(builder.Configuration["RabbitMq:Username"] ?? string.Empty);
			h.Password(builder.Configuration["RabbitMq:Password"] ?? string.Empty);
		});
	});
});
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(cfg =>{
	cfg.SwaggerDoc("v1", new OpenApiInfo { Title = "LGDXRobot Cloud", Version = "v1" });
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
builder.Services.AddDbContextPool<LgdxContext>(cfg => 
  cfg.UseNpgsql(builder.Configuration["PGSQLConnectionString"])
	.LogTo(Console.WriteLine, LogLevel.Information)
	.EnableSensitiveDataLogging()
	.EnableDetailedErrors()
);
builder.Services.AddHttpContextAccessor();

/*
 * Authentication
 */
builder.Services.AddIdentity<LgdxUser, LgdxRole>()
  .AddEntityFrameworkStores<LgdxContext>()
	.AddTokenProvider<AuthenticatorTokenProvider<LgdxUser>>(TokenOptions.DefaultAuthenticatorProvider);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(cfg =>
	{
		cfg.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["LGDXRobotCloudSecret:LgdxUserJwtIssuer"],
			ValidAudience = builder.Configuration["LGDXRobotCloudSecret:LgdxUserJwtIssuer"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["LGDXRobotCloudSecret:LgdxUserJwtSecret"] ?? string.Empty)),
			ClockSkew = TimeSpan.Zero
		};
	});
builder.Services.AddTransient<ValidateRobotClientsCertificate>();
builder.Services.AddAuthentication(LgdxRobotCloudAuthenticationSchemes.RobotClientsCertificateScheme)
	.AddCertificate(LgdxRobotCloudAuthenticationSchemes.RobotClientsCertificateScheme, cfg =>
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
				var validatior = ctx.HttpContext.RequestServices.GetService<ValidateRobotClientsCertificate>();
				if (!await validatior!.Validate(ctx.ClientCertificate, Guid.Parse(guid)))
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
builder.Services.AddAuthentication(LgdxRobotCloudAuthenticationSchemes.RobotClientsJwtScheme)
	.AddJwtBearer(LgdxRobotCloudAuthenticationSchemes.RobotClientsJwtScheme, cfg =>
	{
		cfg.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["LGDXRobotCloudSecret:RobotClientsJwtIssuer"],
			ValidAudience = builder.Configuration["LGDXRobotCloudSecret:RobotClientsJwtIssuer"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["LGDXRobotCloudSecret:RobotClientsJwtSecret"] ?? string.Empty)),
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
builder.Services.AddScoped<IAuthorizationHandler, RobotClientShouldOnlineHandler>();
builder.Services.AddAuthorizationBuilder()
	.AddPolicy("RobotClientShouldOnline", policyBuilder =>
	{
		policyBuilder.RequireAuthenticatedUser();
		policyBuilder.AddRequirements(new RobotClientShouldOnlineRequirement());
	});

/*
 * LGDX Depency Injection
 */
// Administrator
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<IRobotCertificateService, RobotCertificateService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();

// Automation
builder.Services.AddScoped<IAutoTaskService, AutoTaskService>();
builder.Services.AddScoped<IFlowService, FlowService>();
builder.Services.AddScoped<IProgressService, ProgressService>();

// Identity
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Automation
builder.Services.AddScoped<IRealmService, RealmService>();
builder.Services.AddScoped<IWaypointService, WaypointService>();
builder.Services.AddScoped<IRobotService, RobotService>();

// Custom Services
builder.Services.AddScoped<ITriggerRetryService, TriggerRetryService>();
builder.Services.AddScoped<ITriggerService, TriggerService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IEventService, EventService>();
builder.Services.AddScoped<IAutoTaskSchedulerService, AutoTaskSchedulerService>();
builder.Services.AddScoped<IOnlineRobotsService, OnlineRobotsService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

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
app.UseLgdxExpectionHandling();

app.Run();
