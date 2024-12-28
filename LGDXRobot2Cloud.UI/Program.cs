using System.Reflection;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.UI.Authorisation;
using LGDXRobot2Cloud.UI.Components;
using LGDXRobot2Cloud.UI.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

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
		cfg.ConfigureEndpoints(context);
	});
});
builder.Services.AddMemoryCache();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add API
var configureAction = (HttpClient client) => 
  { 
		client.BaseAddress = new Uri(builder.Configuration["Lgdxobot2CloudApiUrl"] ?? string.Empty);
	};
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Navigation
builder.Services.AddHttpClient<IAutoTaskService, AutoTaskService>(configureAction);
builder.Services.AddHttpClient<IFlowService, FlowService>(configureAction);
builder.Services.AddHttpClient<IProgressService, ProgressService>(configureAction);
builder.Services.AddHttpClient<ITriggerService, TriggerService>(configureAction);
builder.Services.AddHttpClient<IWaypointService, WaypointService>(configureAction);
builder.Services.AddHttpClient<IRealmService, RealmService>(configureAction);

// Robot
builder.Services.AddHttpClient<IRobotService, RobotService>(configureAction);
builder.Services.AddScoped<IRobotDataService, RobotDataService>();

// Setting
builder.Services.AddHttpClient<IApiKeyService, ApiKeyService>(configureAction);
builder.Services.AddHttpClient<IRobotCertificateService, RobotCertificateService>(configureAction);
builder.Services.AddHttpClient<IRoleService, RoleService>(configureAction);
builder.Services.AddHttpClient<IUsersService, UsersService>(configureAction);

// Identity
builder.Services.AddHttpClient<IAuthService, AuthService>(configureAction);
builder.Services.AddHttpClient<IUserService, UserService>(configureAction);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(cfg =>
	{
		cfg.LoginPath = "/login";
	});
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

// Policy
builder.Services.AddScoped<IAuthorizationHandler, ValidateLgdxUserAccessHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, ValidateLgdxUserAccesPolicyProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
	return TypedResults.LocalRedirect("/Login");
}).RequireAuthorization();

app.Run();
