using Mairie.API.Helpers;
using Mairie.API.Infrastructure.Security;
using Mairie.DAL.Configuration;
using Mairie.DAL.Repositories;
using Mairie.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Server.HttpSys;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Configuration de l'authentification Windows
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

 

// Configuration des politiques d'autorisation basées sur les rôles
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
    options.AddPolicy("Agent", p => p.RequireClaim(ClaimTypes.Role, "Agent"));
    options.AddPolicy("ChefService", p => p.RequireClaim(ClaimTypes.Role, "ChefService"));
    options.AddPolicy("Administrateur", p => p.RequireClaim(ClaimTypes.Role,"Administrateur"));
});

// Récupération de la chaîne de connexion depuis les configurations 
string connectionString =builder.Configuration["DefaultConnection"]?? builder.Configuration    ["ConnectionStrings:DefaultConnection"]?? 
     throw new InvalidOperationException("Connection string 'DefaultConnection' introuvable");
// Add services to the container.
// Enregistrement de la configuration de la base de données avec la chaîne de connexion décryptée
builder.Services.AddSingleton(new DatabaseConfiguration(SecretManager.Decrypt(connectionString)));
// Enregistrement des repositories
builder.Services.AddScoped<IDemandeRepository, DemandeRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
// Enregistrement de l'accessor pour le contexte HTTP et récupération des informations utilisateur 
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddScoped<IClaimsTransformation, RoleClaimsTransformation>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run(); 
