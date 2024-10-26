using LGDXRobot2Cloud.UI.Authorisation;
using LGDXRobot2Cloud.UI.Components;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add API
var configureAction = (HttpClient client) => 
    { client.BaseAddress = new Uri(builder.Configuration["Lgdxobot2CloudApiUrl"] ?? string.Empty); };
// Navigation
builder.Services.AddHttpClient<IAutoTaskService, AutoTaskService>(configureAction);
builder.Services.AddHttpClient<IFlowService, FlowService>(configureAction);
builder.Services.AddHttpClient<IProgressService, ProgressService>(configureAction);
builder.Services.AddHttpClient<ITriggerService, TriggerService>(configureAction);
builder.Services.AddHttpClient<IWaypointService, WaypointService>(configureAction);

// Robot
builder.Services.AddHttpClient<INodeService, NodeService>(configureAction);
builder.Services.AddHttpClient<INodesCollectionService, NodesCollectionService>(configureAction);
builder.Services.AddHttpClient<IRobotService, RobotService>(configureAction);

// Setting
builder.Services.AddHttpClient<IApiKeyService, ApiKeyService>(configureAction);
builder.Services.AddHttpClient<IRobotCertificateService, RobotCertificateService>(configureAction);

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Identity
builder.Services.AddHttpClient<IUserService, UserService>(configureAction);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(cfg =>
	{
		cfg.LoginPath = "/login";
	});
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

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
});

app.Run();
