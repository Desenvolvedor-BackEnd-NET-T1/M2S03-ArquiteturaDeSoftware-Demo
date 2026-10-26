namespace Pizzaria.Monolito.Models;

public class Pedido
{
    public int Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public List<ItemPedido> Itens { get; set; } = [];
    public decimal Total => Itens.Sum(item => item.Subtotal);
    public StatusPedido Status { get; set; } = StatusPedido.Recebido;
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}

public class ItemPedido
{
    public int PizzaId { get; set; }
    public string NomePizza { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal => PrecoUnitario * Quantidade;
}
