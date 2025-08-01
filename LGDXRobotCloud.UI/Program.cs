using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using LGDXRobotCloud.UI;
using LGDXRobotCloud.UI.Authorisation;
using LGDXRobotCloud.UI.Components;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Consumers;
using LGDXRobotCloud.UI.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
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
		cfg.ReceiveEndpoint(new TemporaryEndpointDefinition(), e =>
		{
			e.ConfigureConsumer<RobotCommandsConsumer>(context);
		});
		cfg.ReceiveEndpoint(new TemporaryEndpointDefinition(), e =>
		{
			e.ConfigureConsumer<RobotDataConsumer>(context);
		});
		cfg.ReceiveEndpoint(new TemporaryEndpointDefinition(), e =>
		{
			e.ConfigureConsumer<AutoTaskUpdateConsumer>(context);
		});
		cfg.ConfigureEndpoints(context);
	});
});
builder.Services.AddMemoryCache();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add API
var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
store.Open(OpenFlags.ReadOnly);
var certificate = store.Certificates.First(cert => cert.SerialNumber.Contains(builder.Configuration["LGDXRobotCloudAPI:CertificateSN"]!));
var url = new Uri(builder.Configuration["LGDXRobotCloudAPI:Url"] ?? string.Empty);

builder.Services.AddKiotaHandlers();
builder.Services.AddScoped<LgdxApiClientFactory>();
builder.Services.AddHttpClient<LgdxApiClientFactory>((sp, client) => 
{
  client.BaseAddress = url;
})
	.AddHttpMessageHandler<HeadersInspectionHandler>()
	.ConfigurePrimaryHttpMessageHandler(() => {
			var handler = new HttpClientHandler
			{
				AllowAutoRedirect = true,
				UseDefaultCredentials = true
			};
			handler.ClientCertificates.Add(certificate);
			return handler;
	})
	.AttachKiotaHandlers();
builder.Services.AddScoped(sp => sp.GetRequiredService<LgdxApiClientFactory>().GetClient());
builder.Services.AddHttpClient<IRefreshTokenService, RefreshTokenService>(client =>
{ 
	client.BaseAddress = url;
})
	.ConfigurePrimaryHttpMessageHandler(() => {
			var handler = new HttpClientHandler
			{
				AllowAutoRedirect = true,
				UseDefaultCredentials = true
			};
			handler.ClientCertificates.Add(certificate);
			return handler;
	});
builder.Services.AddScoped<ICachedRealmService, CachedRealmService>();
builder.Services.AddScoped<IRobotDataService, RobotDataService>();
builder.Services.AddSingleton<IRealTimeService, RealTimeService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(cfg =>
	{
		cfg.LoginPath = AppRoutes.Identity.Login;
		cfg.ExpireTimeSpan = TimeSpan.FromMinutes(int.Parse(builder.Configuration["LGDXRobotCloudUI:SessionTimeoutMinutes"] ?? "15"));
		cfg.SlidingExpiration = true;
	});
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, LgdxAuthenticationStateProvider>();

// Policy
builder.Services.AddScoped<IAuthorizationHandler, ValidateLgdxUserAccessHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, ValidateLgdxUserAccesPolicyProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.MapPost("api/Logout", async (HttpContext context) =>
{
	await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
	return TypedResults.LocalRedirect(AppRoutes.Identity.Login);
}).RequireAuthorization();

app.Run();
