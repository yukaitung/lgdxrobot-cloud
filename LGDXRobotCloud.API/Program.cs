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
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
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
builder.Services.AddOpenApi(options =>
{
	options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
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
	.AddTokenProvider<AuthenticatorTokenProvider<LgdxUser>>(TokenOptions.DefaultAuthenticatorProvider)
	.AddTokenProvider<DataProtectorTokenProvider<LgdxUser>>(TokenOptions.DefaultProvider);
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
				var claims = new[] {
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
	app.MapOpenApi();
	app.MapScalarApiReference();
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

internal sealed class BearerSecuritySchemeTransformer(
	IAuthenticationSchemeProvider authenticationSchemeProvider,
	IServer server
) : IOpenApiDocumentTransformer
{
	public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
	{
		var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
		if (authenticationSchemes.Any(authScheme => authScheme.Name == JwtBearerDefaults.AuthenticationScheme))
		{
			var requirements = new Dictionary<string, OpenApiSecurityScheme>
			{
				["Bearer"] = new OpenApiSecurityScheme
				{
					Type = SecuritySchemeType.Http,
					Scheme = "bearer", // "bearer" refers to the header name here
					In = ParameterLocation.Header,
					BearerFormat = "Json Web Token"
				}
			};
			document.Components ??= new OpenApiComponents();
			document.Components.SecuritySchemes = requirements;
		}
		document.Info = new()
		{
			Title = "LGDXRobot Cloud API",
			Version = "v1",
			Description = "Core API for the LGDXRobot Cloud.",
			Contact = new OpenApiContact
			{
				Name = "LGDXRobot",
				Url = new Uri("https://lgdxrobot.bristolgram.uk"),
			},
			License = new OpenApiLicense
			{
				Name = "The MIT License",
				Url = new Uri("https://opensource.org/license/MIT")
			}
		};
		var address = server.Features.Get<IServerAddressesFeature>()!.Addresses.LastOrDefault(); // HTTPS port must higher
		address = address!.Replace("[::]", "localhost");
		document.Servers =
    [
      new()
			{
				Url = address,
				Description = "Default Server"
			}
		];
	}
}