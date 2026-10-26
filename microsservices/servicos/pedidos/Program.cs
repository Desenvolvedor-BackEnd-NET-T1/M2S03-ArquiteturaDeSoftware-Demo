using Pizzaria.Pedidos.Api.Clientes;
using Pizzaria.Pedidos.Api.Data;

// Microsserviço de Pedidos: dono dos pedidos (cliente, itens, preços cobrados e total).
// Para criar um pedido ele CONVERSA com outros serviços pela rede:
//   - Cardápio (5101): para validar as pizzas e obter os preços atuais;
//   - Cozinha  (5103): para enviar a comanda de preparo.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<PedidoRepositorio>();

// Clientes HTTP tipados: o endereço de cada serviço vem do appsettings.json.
// O timeout curto faz a falha de um serviço aparecer rápido, em vez de travar a requisição.
builder.Services.AddHttpClient<CardapioCliente>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Servicos:CardapioUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddHttpClient<CozinhaCliente>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Servicos:CozinhaUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

var origensPermitidas = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(origensPermitidas).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
