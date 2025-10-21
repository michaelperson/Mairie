using Mairie.App.Components;
using Mairie.App.Services;
using Mairie.App.Services.Interfaces;
using Mairie.Appp.Services;
using Microsoft.AspNetCore.Authentication.Negotiate;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ===== Configuration du HttpClient avec authentification Windows =====
ApiClientConfiguration.ConfigureHttpClient(builder.Services, builder.Configuration);

// ===== Enregistrement des services métier =====
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDemandesService, DemandesService>();

// ===== Configuration du logging =====
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// En production, configurer un système de logging approprié
if (builder.Environment.IsProduction())
{
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// ===== Configuration HTTPS (Security by Design) =====
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    // Protection contre le clickjacking
    context.Response.Headers.Append("X-Frame-Options", "DENY");

    // Protection XSS
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    // Protection contre le sniffing MIME
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

    // Content Security Policy
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdnjs.cloudflare.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "font-src 'self' https://cdn.jsdelivr.net; " +
        "img-src 'self' data: https:;");

    // Referrer Policy
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    await next();
});
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization(); 
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
