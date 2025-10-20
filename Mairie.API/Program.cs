//Code temporaire qui sera supprimé dès que les secrets seront gérés correctement
//!!!!!!NE PAS LAISSER CE CODE POUR LE GIT!!!!!!!
//using Mairie.API.Helpers;

//string secretValue = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MairieDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
//string encryptedSecret = SecretManager.Encrypt(secretValue);
//Console.WriteLine($"Encrypted Secret: {encryptedSecret}");


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Add services to the container.
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
