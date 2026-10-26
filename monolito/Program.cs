using System.Text.Json.Serialization;
using Pizzaria.Monolito.Data;
using Pizzaria.Monolito.Services;

// Monólito: interface web, API e regras de negócio rodam no MESMO processo.
// Um único "dotnet run" sobe tudo, e um único deploy publica tudo.
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// "Banco de dados" em memória compartilhado por todas as funcionalidades.
// Singleton = uma única instância enquanto a aplicação estiver rodando.
builder.Services.AddSingleton<BancoEmMemoria>();
builder.Services.AddSingleton<CardapioService>();
builder.Services.AddSingleton<PedidoService>();

var app = builder.Build();

// Serve a interface (wwwroot/index.html, css e js) pela própria aplicação.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
