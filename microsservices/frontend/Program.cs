// Frontend: só entrega a interface web (HTML, CSS e JS). Não tem regras de negócio nem dados.
// O JavaScript, rodando no navegador, chama diretamente a API de cada microsserviço.
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Informa ao navegador onde está cada serviço (valores do appsettings.json),
// assim os endereços não ficam fixos no JavaScript.
app.MapGet("/config", (IConfiguration configuracao) => new
{
    cardapioUrl = configuracao["Servicos:CardapioUrl"],
    pedidosUrl = configuracao["Servicos:PedidosUrl"],
    cozinhaUrl = configuracao["Servicos:CozinhaUrl"]
});

app.Run();
