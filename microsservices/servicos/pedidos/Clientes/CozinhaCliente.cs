using System.Net.Http.Json;
using Pizzaria.Pedidos.Api.Models;

namespace Pizzaria.Pedidos.Api.Clientes;

public class CozinhaCliente(HttpClient http)
{
    public async Task EnviarComandaAsync(Pedido pedido, CancellationToken cancelamento)
    {
        var comanda = new
        {
            pedidoId = pedido.Id,
            cliente = pedido.Cliente,
            itens = pedido.Itens.Select(i => new { nomePizza = i.NomePizza, quantidade = i.Quantidade })
        };

        try
        {
            var resposta = await http.PostAsJsonAsync("api/comandas", comanda, cancelamento);
            resposta.EnsureSuccessStatusCode();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ServicoIndisponivelException(
                "Cozinha",
                $"Não foi possível enviar a comanda ao serviço de Cozinha em {http.BaseAddress}: {ex.Message}",
                ex);
        }
    }
}
