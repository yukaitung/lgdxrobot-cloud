using System.Reflection;
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
builder.Configuration.AddJsonFile("secrets.json", true, true);
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
builder.Services.AddKiotaHandlers();
builder.Services.AddHttpClient<LgdxApiClientFactory>((sp, client) => {
  client.BaseAddress = new Uri(builder.Configuration["LGDXRobotCloudAPIUrl"] ?? string.Empty);
})
	.AddHttpMessageHandler(() => new HeadersInspectionHandler())
	.AttachKiotaHandlers();
builder.Services.AddTransient(sp => sp.GetRequiredService<LgdxApiClientFactory>().GetClient());
var configureAction = (HttpClient client) => 
  { 
		client.BaseAddress = new Uri(builder.Configuration["LGDXRobotCloudAPIUrl"] ?? string.Empty);
	};

builder.Services.AddHttpClient<IRefreshTokenService, RefreshTokenService>(configureAction);
builder.Services.AddScoped<ICachedRealmService, CachedRealmService>();
builder.Services.AddScoped<IRobotDataService, RobotDataService>();
builder.Services.AddSingleton<IRealTimeService, RealTimeService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(cfg =>
	{
		cfg.LoginPath = AppRoutes.Identity.Login;
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

app.UseHttpsRedirection();

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
