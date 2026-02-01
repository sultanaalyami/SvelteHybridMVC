using HRCE.Core;
using HRCE.Infrastructure.Extensions;
using HRCE.Data;
using HRCE.Services.HRCE;
using HRCE.Services.Node;
using HRCE.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SvelteRendererCore = HRCE.Core.ISvelteRenderer;
using NLog.Web;
using HRCE.Services.Logging;
using HRCE.Services.UiRules;
using HRCE.Services.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

// 1. تكوين MVC مع Svelte و Runtime Compilation
var mvcBuilder = builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});
mvcBuilder.AddRazorRuntimeCompilation();

// 2. خدمات Svelte الأساسية
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableSSR = true;
    options.HydrationMode = HydrationMode.Selective;
    options.WatchForChanges = builder.Environment.IsDevelopment();
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// تسجيل خدمات Node.js SSR
builder.Services.AddHttpClient<INodeService, NodeService>();
builder.Services.AddHttpContextAccessor(); // مطلوب للتحقق من المستخدم

builder.Services.Configure<LogLearningOptions>(builder.Configuration.GetSection("LogLearning"));
builder.Services.AddSingleton(provider => provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LogLearningOptions>>().Value);
builder.Services.AddSingleton<LogFileTailer>();
builder.Services.AddSingleton<LogCorrelationStore>();
builder.Services.AddSingleton<LogStore>();
builder.Services.AddHostedService<LogLearningService>();

builder.Services.Configure<UiRuleOptions>(builder.Configuration.GetSection("UiRules"));
var uiRulesDbPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "ui-rules.db");
var uiRulesConnection = builder.Configuration.GetConnectionString("UiRulesConnection")
    ?? $"Data Source={uiRulesDbPath}";
builder.Services.AddDbContext<UiRulesDbContext>(options =>
    options.UseSqlite(uiRulesConnection));
builder.Services.AddScoped<UiRuleStore>();
builder.Services.AddScoped<UiRuleEvaluator>();
builder.Services.AddHostedService<UiRulesInitializer>();

builder.Services.Configure<BuilderOptions>(options =>
{
    options.RootPath = builder.Environment.ContentRootPath;
});
builder.Services.AddSingleton<PageBuilder>();

// ✨ تسجيل نظام المراقبة متعدد المنصات (eBPF/ETW/DTrace)
builder.Services.AddPlatformMonitoring(options =>
{
    options.AutoStart = true;
    options.EnableStats = true;
    options.EnableEventLogging = builder.Environment.IsDevelopment();
    options.UseRealMonitor = false; // تعطيل المراقب الحقيقي مؤقتاً - يسبب تجمد
});

// ✨ تسجيل خدمات HRCE - Hybrid Razor Component Engine
builder.Services.AddHRCE(options =>
{
    options.EnableSvelteSSR = true;
    options.ComponentBasePath = "~/Components";
    options.IsolationMode = IsolationMode.Full;
    options.DefaultRenderer = RendererType.Svelte;
    options.EnableHotReload = builder.Environment.IsDevelopment();
    options.Authorization.DefaultPolicy = "AuthenticatedUser";
    options.Authorization.FallbackComponent = "_Unauthorized";
});

var app = builder.Build();

// Log platform information
var platformService = app.Services.GetRequiredService<HRCE.Core.Platform.IPlatformDetectionService>();
var platformInfo = platformService.GetPlatformInfo();
app.Logger.LogInformation("Running on {OS} ({Arch})", 
    platformInfo.OSDescription, 
    platformInfo.Architecture);
app.Logger.LogInformation("Monitoring System: {MonitoringSystem}", 
    platformInfo.MonitoringSystem);
app.Logger.LogInformation("Container: {IsContainer}, Kernel: {KernelVersion}", 
    platformInfo.IsContainer, 
    platformInfo.KernelVersion);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// ✨ تفعيل HRCE Middleware لمعالجة المكونات الهجينة
app.UseHRCE();

app.UseRouting();
app.UseAuthorization();
app.UseStaticFiles();

// Platform information endpoint
app.MapGet("/api/platform", (HRCE.Core.Platform.IPlatformDetectionService service) =>
{
    return Results.Ok(service.GetPlatformInfo());
});

// Monitoring stats endpoint
app.MapGet("/api/monitoring/stats", async (HRCE.Core.Monitoring.IKernelMonitor monitor) =>
{
    var stats = await monitor.GetStatsAsync();
    return Results.Ok(stats);
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy", 
    timestamp = DateTime.UtcNow 
}));

// Svelte SSR endpoint
app.MapGet("/_svelte/ssr/{component}", async (string component, SvelteRendererCore renderer) =>
{
    var result = await renderer.RenderAsync(component);
    return Results.Content(result, "text/html");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
