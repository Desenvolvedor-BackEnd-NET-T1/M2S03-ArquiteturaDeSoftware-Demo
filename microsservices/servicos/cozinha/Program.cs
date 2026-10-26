using System.Text.Json.Serialization;
using Pizzaria.Cozinha.Api.Data;

// Microsserviço de Cozinha: dono do andamento (status) de preparo de cada pedido.
// Recebe comandas do serviço de Pedidos e é consultado pelo frontend, sempre via HTTP.
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<ComandaRepositorio>();

var origensPermitidas = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(origensPermitidas).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
