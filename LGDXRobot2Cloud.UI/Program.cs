using LGDXRobot2Cloud.UI;
using LGDXRobot2Cloud.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add API
var configureAction = (HttpClient client) => 
    { client.BaseAddress = new Uri(builder.Configuration["Lgdxobot2CloudApiUrl"] ?? throw new Exception("The Lgdxobot2CloudApiUrl is missing.")); };
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

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
