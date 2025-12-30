using SvelteHybridMVC.Core;
using SvelteHybridMVC.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. تكوين MVC مع Svelte و Runtime Compilation
var mvcBuilder = builder.Services.AddControllersWithViews();
mvcBuilder.AddRazorRuntimeCompilation();

// إضافة حماية من CSRF
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// 2. خدمات Svelte الأساسية
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableSSR = true;
    options.HydrationMode = HydrationMode.Selective;
    options.WatchForChanges = builder.Environment.IsDevelopment();
});

// إضافة HSTS
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// إجبار HTTPS
app.UseHttpsRedirection();

// إضافة رؤوس الأمان
app.Use(async (context, next) =>
{
    // منع Clickjacking
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    
    // منع MIME type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    
    // حماية XSS في المتصفحات القديمة
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    
    // سياسة أمان المحتوى
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self'");
    
    // سياسة الإحالة
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // سياسة الأذونات
    context.Response.Headers.Append("Permissions-Policy", 
        "geolocation=(), microphone=(), camera=()");
    
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Svelte SSR endpoint
app.MapGet("/_svelte/ssr/{component}", async (string component, ISvelteRenderer renderer) =>
{
    var result = await renderer.RenderAsync(component);
    return Results.Content(result, "text/html");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
