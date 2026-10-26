using Pizzaria.Cardapio.Api.Data;

// Microsserviço de Cardápio: único dono dos dados de produtos e preços.
// Roda em seu próprio processo e porta (5101) e é publicado de forma independente.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<CardapioRepositorio>();

// O frontend roda em outra origem (porta 5100), então o navegador exige CORS.
var origensPermitidas = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(origensPermitidas).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
