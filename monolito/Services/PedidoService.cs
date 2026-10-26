using Pizzaria.Monolito.Data;
using Pizzaria.Monolito.Models;

namespace Pizzaria.Monolito.Services;

public class PedidoService(BancoEmMemoria banco, CardapioService cardapio)
{
    public IReadOnlyList<Pedido> ListarPedidos()
    {
        lock (banco.Trava)
        {
            return banco.Pedidos.OrderByDescending(pedido => pedido.Id).ToList();
        }
    }

    public Pedido? BuscarPedido(int id)
    {
        lock (banco.Trava)
        {
            return banco.Pedidos.FirstOrDefault(pedido => pedido.Id == id);
        }
    }

    // Retorna o pedido criado ou uma mensagem de erro de validação.
    public (Pedido? Pedido, string? Erro) CriarPedido(NovoPedidoRequest requisicao)
    {
        if (string.IsNullOrWhiteSpace(requisicao.Cliente))
            return (null, "Informe o nome do cliente.");

        if (requisicao.Itens.Count == 0)
            return (null, "O pedido precisa ter pelo menos uma pizza.");

        var itens = new List<ItemPedido>();
        foreach (var item in requisicao.Itens)
        {
            if (item.Quantidade <= 0)
                return (null, "A quantidade de cada pizza deve ser maior que zero.");

            // No monólito, consultar o cardápio é só uma chamada de método no mesmo processo:
            // não há rede envolvida, então não existe "cardápio fora do ar".
            var pizza = cardapio.BuscarPizza(item.PizzaId);
            if (pizza is null)
                return (null, $"A pizza {item.PizzaId} não existe no cardápio.");

            itens.Add(new ItemPedido
            {
                PizzaId = pizza.Id,
                NomePizza = pizza.Nome,
                Quantidade = item.Quantidade,
                PrecoUnitario = pizza.Preco
            });
        }

        lock (banco.Trava)
        {
            var agora = DateTime.Now;
            var pedido = new Pedido
            {
                Id = banco.ProximoIdPedido++,
                Cliente = requisicao.Cliente.Trim(),
                Itens = itens,
                Status = StatusPedido.Recebido,
                CriadoEm = agora,
                AtualizadoEm = agora
            };
            banco.Pedidos.Add(pedido);
            return (pedido, null);
        }
    }

    public Pedido? AtualizarStatus(int id, StatusPedido novoStatus)
    {
        lock (banco.Trava)
        {
            var pedido = banco.Pedidos.FirstOrDefault(p => p.Id == id);
            if (pedido is null)
                return null;

            pedido.Status = novoStatus;
            pedido.AtualizadoEm = DateTime.Now;
            return pedido;
        }
    }
}
