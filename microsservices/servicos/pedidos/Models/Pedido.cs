namespace Pizzaria.Pedidos.Api.Models;

// Repare que o pedido NÃO tem status: o andamento do preparo pertence ao serviço de Cozinha.
// Cada serviço guarda somente os dados pelos quais é responsável.
public class Pedido
{
    public int Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public List<ItemPedido> Itens { get; set; } = [];
    public decimal Total => Itens.Sum(item => item.Subtotal);
    public DateTime CriadoEm { get; set; }
}

public class ItemPedido
{
    public int PizzaId { get; set; }
    public string NomePizza { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal => PrecoUnitario * Quantidade;
}

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
