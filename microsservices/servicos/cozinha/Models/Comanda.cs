namespace Pizzaria.Cozinha.Api.Models;

// A cozinha guarda apenas o que precisa para preparar a pizza:
// não conhece preços nem totais (isso pertence aos serviços de Cardápio e Pedidos).
public class Comanda
{
    public int PedidoId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public List<ItemComanda> Itens { get; set; } = [];
    public StatusPreparo Status { get; set; } = StatusPreparo.Recebido;
    public DateTime RecebidaEm { get; set; }
    public DateTime AtualizadaEm { get; set; }
}

public class ItemComanda
{
    public string NomePizza { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}

public enum StatusPreparo
{
    Recebido,
    EmPreparo,
    NoForno,
    Pronto,
    Entregue
}

public class NovaComandaRequest
{
    public int PedidoId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public List<ItemComanda> Itens { get; set; } = [];
}

public class AtualizarStatusRequest
{
    public StatusPreparo Status { get; set; }
}
