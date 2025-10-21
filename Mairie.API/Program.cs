using Mairie.API.Helpers;
using Mairie.API.Infrastructure.Security;
using Mairie.DAL.Configuration;
using Mairie.DAL.Services;
using Mairie.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.HttpSys;
using OwaspHeaders.Core.Extensions;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Configuration de l'authentification Windows
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

 

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
    options.AddPolicy("AdminOrAgent", policy =>
    policy.RequireRole("Administrateur", "Agent"));

    // Politique personnalisée pour vérifier la propriété d'une demande
    options.AddPolicy("DemandeOwnerOrAbove", policy =>
    {
        policy.Requirements.Add(new OwnsDemandeRequirement("DemandeOwnerOrAbove"));
    });
});

// Récupération de la chaîne de connexion depuis les configurations 
string connectionString =builder.Configuration["DefaultConnection"]?? builder.Configuration    ["ConnectionStrings:DefaultConnection"]?? 
     throw new InvalidOperationException("Connection string 'DefaultConnection' introuvable"); 
// Add services to the container.
// Enregistrement de la configuration de la base de données avec la chaîne de connexion décryptée
builder.Services.AddSingleton(new DatabaseConfiguration(SecretManager.Decrypt(connectionString).Replace("MairieDB", "MairieDB_test")));
// Enregistrement des repositories
builder.Services.AddScoped<IDemandeRepository, DemandeRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
// Enregistrement de l'accessor pour le contexte HTTP et récupération des informations utilisateur 
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

// Enregistrement du gestionnaire de le mapping entre rôles et claims
builder.Services.AddScoped<IClaimsTransformation, RoleClaimsTransformation>();

// Enregistrement du gestionnaire pour OwnsDemandeRequirement
builder.Services.AddScoped<IAuthorizationHandler, OwnsDemandeHandler>();

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
app.Run();
