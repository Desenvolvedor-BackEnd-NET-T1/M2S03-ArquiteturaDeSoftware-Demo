using Pizzaria.Pedidos.Api.Models;

namespace Pizzaria.Pedidos.Api.Data;

// Armazenamento em memória exclusivo do serviço de Pedidos. Reiniciar o serviço apaga os pedidos.
public class PedidoRepositorio
{
    private readonly object _trava = new();
    private readonly List<Pedido> _pedidos = [];
    private int _proximoId = 1;

    public IReadOnlyList<Pedido> Listar()
    {
        lock (_trava)
        {
            return _pedidos.OrderByDescending(p => p.Id).ToList();
        }
    }

    public Pedido? Buscar(int id)
    {
        lock (_trava)
        {
            return _pedidos.FirstOrDefault(p => p.Id == id);
        }
    }

    public Pedido Adicionar(string cliente, List<ItemPedido> itens)
    {
        lock (_trava)
        {
            var pedido = new Pedido
            {
                Id = _proximoId++,
                Cliente = cliente,
                Itens = itens,
                CriadoEm = DateTime.Now
            };
            _pedidos.Add(pedido);
            return pedido;
        }
    }

    public void Remover(int id)
    {
        lock (_trava)
        {
            _pedidos.RemoveAll(p => p.Id == id);
        }
    }
}
