using Mairie.API.Helpers;
using Mairie.API.Infrastructure.Security;
using Mairie.API.Middleware;
using Mairie.DAL.Configuration;
using Mairie.DAL.Services;
using Mairie.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.HttpSys;
using OwaspHeaders.Core.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
// Récupération de la chaîne de connexion depuis les configurations 
string connectionString = builder.Configuration["DefaultConnection"] ?? builder.Configuration["ConnectionStrings:DefaultConnection"] ??
     throw new InvalidOperationException("Connection string 'DefaultConnection' introuvable"); 
// Configuration Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
  //  .Filter.ByIncludingOnly(evt => evt.Level <= LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "MairieApi")
    // Sink vers fichier réseau avec rollup
    .WriteTo.File(
        path: builder.Configuration["Logging:FilePath"] ?? @"\\serveur\logs\app-.log",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,
        fileSizeLimitBytes: 10 * 1024 * 1024, // 10 MB
        retainedFileCountLimit: 30, // Garde 30 jours de logs
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{MachineName}] [{ThreadId}] {Message:lj}{NewLine}{Exception}")
    // Console pour le développement
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddControllers();
// Configuration de l'authentification Windows
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();


// Enregistrement du gestionnaire pour OwnsDemandeRequirement
builder.Services.AddScoped<IAuthorizationHandler, OwnsDemandeHandler>();

// Configuration des politiques d'autorisation basées sur les rôles
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.FallbackPolicy = options.DefaultPolicy;
    options.AddPolicy("Agent", p => p.RequireClaim(ClaimTypes.Role, "Agent"));
    options.AddPolicy("ChefService", p => p.RequireClaim(ClaimTypes.Role, "ChefService"));
    options.AddPolicy("Administrateur", p => p.RequireClaim(ClaimTypes.Role,"Administrateur"));
    options.AddPolicy("AdminOrAgent", policy => policy.RequireClaim(ClaimTypes.Role, "Administrateur", "Agent"));

    // Politique personnalisée pour vérifier la propriété d'une demande
    options.AddPolicy("DemandeOwnerOrAbove", policy =>
    {
        policy.Requirements.Add(new OwnsDemandeRequirement("DemandeOwnerOrAbove"));
    });
});


// Add services to the container.
// Enregistrement de la configuration de la base de données avec la chaîne de connexion décryptée
builder.Services.AddSingleton(new DatabaseConfiguration(SecretManager.Decrypt(connectionString).Replace("MairieDB", "MairieDB_test")));
// Enregistrement des repositories
builder.Services.AddScoped<IDemandeRepository, DemandeRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();
// Enregistrement de l'accessor pour le contexte HTTP et récupération des informations utilisateur 
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

// Enregistrement du gestionnaire de le mapping entre rôles et claims
builder.Services.AddScoped<IClaimsTransformation, RoleClaimsTransformation>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Version = "v1",
            Title = "API Mairie",
            Description = "API interne de la Mairie"
        });
        c.EnableAnnotations();
        c.SchemaFilter<EnumSchemaFilter>();
    }



    );
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});




var app = builder.Build();
//Gestion globale des erreurs 
app.UseMiddleware<ErrorHandlingMiddleware>();

// Middleware de logging des requêtes
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error
        : elapsed > 5000
            ? LogEventLevel.Warning
            : LogEventLevel.Information;
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        diagnosticContext.Set("UserName", httpContext.User.Identity?.Name ?? "Anonymous");
    };
});

//En production, l'application utilise HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
    // Recommandé pour la production pour forcer HTTPS
    app.UseHsts();
}
// Ajoute des en-têtes comme X-Frame-Options, X-Content-Type-Options, Referrer-Policy, etc.
app.UseSecureHeadersMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
try
{
    Log.Information("Démarrage de l'application Web API - Mairie Api");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "L'application a échoué au démarrage");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
