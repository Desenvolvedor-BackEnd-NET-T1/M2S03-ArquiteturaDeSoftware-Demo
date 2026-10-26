using System.Net.Http.Json;

namespace Pizzaria.Pedidos.Api.Clientes;

// Cópia local apenas dos campos do cardápio que o serviço de Pedidos usa.
// Não referenciamos o projeto do Cardápio: o "contrato" entre os serviços é o JSON da API.
public record PizzaDoCardapio(int Id, string Nome, decimal Preco);

public class CardapioCliente(HttpClient http)
{
    public async Task<IReadOnlyList<PizzaDoCardapio>> ListarPizzasAsync(CancellationToken cancelamento)
    {
        try
        {
            var pizzas = await http.GetFromJsonAsync<List<PizzaDoCardapio>>("api/cardapio", cancelamento);
            return pizzas ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ServicoIndisponivelException(
                "Cardápio",
                $"Não foi possível consultar o serviço de Cardápio em {http.BaseAddress}: {ex.Message}",
                ex);
        }
    }
}
