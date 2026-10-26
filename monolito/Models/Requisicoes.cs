namespace Pizzaria.Monolito.Models;

// Dados recebidos da interface web ao criar um pedido.
public class NovoPedidoRequest
{
    public string Cliente { get; set; } = string.Empty;
    public List<ItemNovoPedido> Itens { get; set; } = [];
}

public class ItemNovoPedido
{
    public int PizzaId { get; set; }
    public int Quantidade { get; set; }
}

public class AtualizarStatusRequest
{
    public StatusPedido Status { get; set; }
}
