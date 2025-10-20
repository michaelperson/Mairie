using Mairie.API.Helpers;
using Mairie.DAL.Configuration;
using Mairie.DAL.Repositories;
using Mairie.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();


// Récupération de la chaîne de connexion depuis les configurations 
string connectionString =builder.Configuration["DefaultConnection"]?? builder.Configuration    ["ConnectionStrings:DefaultConnection"]?? 
     throw new InvalidOperationException("Connection string 'DefaultConnection' introuvable");
// Add services to the container.
// Enregistrement de la configuration de la base de données avec la chaîne de connexion décryptée
builder.Services.AddSingleton(new DatabaseConfiguration(SecretManager.Decrypt(connectionString)));
// Enregistrement des repositories
builder.Services.AddScoped<IDemandeRepository, DemandeRepository>();




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

app.MapControllers();
app.Run(); 
