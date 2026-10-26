using Pizzaria.Cozinha.Api.Models;

namespace Pizzaria.Cozinha.Api.Data;

// Armazenamento em memória exclusivo da Cozinha. Reiniciar o serviço apaga as comandas.
public class ComandaRepositorio
{
    private readonly object _trava = new();
    private readonly Dictionary<int, Comanda> _comandas = [];

    public IReadOnlyList<Comanda> Listar()
    {
        lock (_trava)
        {
            return _comandas.Values.OrderByDescending(c => c.PedidoId).ToList();
        }
    }

    public Comanda? Buscar(int pedidoId)
    {
        lock (_trava)
        {
            return _comandas.GetValueOrDefault(pedidoId);
        }
    }

    // Retorna false se já existir comanda para o pedido.
    public bool Adicionar(Comanda comanda)
    {
        lock (_trava)
        {
            return _comandas.TryAdd(comanda.PedidoId, comanda);
        }
    }

    public Comanda? AtualizarStatus(int pedidoId, StatusPreparo status)
    {
        lock (_trava)
        {
            if (!_comandas.TryGetValue(pedidoId, out var comanda))
                return null;

            comanda.Status = status;
            comanda.AtualizadaEm = DateTime.Now;
            return comanda;
        }
    }
}
