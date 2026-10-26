using Microsoft.AspNetCore.Mvc;
using Pizzaria.Pedidos.Api.Clientes;
using Pizzaria.Pedidos.Api.Data;
using Pizzaria.Pedidos.Api.Models;

namespace Pizzaria.Pedidos.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController(
    PedidoRepositorio repositorio,
    CardapioCliente cardapio,
    CozinhaCliente cozinha,
    ILogger<PedidosController> logger) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Pedido>> Listar() => Ok(repositorio.Listar());

    [HttpGet("{id:int}")]
    public ActionResult<Pedido> Buscar(int id)
    {
        var pedido = repositorio.Buscar(id);
        return pedido is null ? NotFound() : Ok(pedido);
    }

    [HttpPost]
    public async Task<ActionResult<Pedido>> Criar(NovoPedidoRequest requisicao, CancellationToken cancelamento)
    {
        if (string.IsNullOrWhiteSpace(requisicao.Cliente))
            return PedidoInvalido("Informe o nome do cliente.");
        if (requisicao.Itens.Count == 0)
            return PedidoInvalido("O pedido precisa ter pelo menos uma pizza.");
        if (requisicao.Itens.Any(i => i.Quantidade <= 0))
            return PedidoInvalido("A quantidade de cada pizza deve ser maior que zero.");

        // 1) Busca os preços no serviço de Cardápio (chamada HTTP, pode falhar!).
        IReadOnlyList<PizzaDoCardapio> pizzas;
        try
        {
            pizzas = await cardapio.ListarPizzasAsync(cancelamento);
        }
        catch (ServicoIndisponivelException ex)
        {
            return ServicoIndisponivel(ex);
        }

        var itens = new List<ItemPedido>();
        foreach (var item in requisicao.Itens)
        {
            var pizza = pizzas.FirstOrDefault(p => p.Id == item.PizzaId);
            if (pizza is null)
                return PedidoInvalido($"A pizza {item.PizzaId} não existe no cardápio.");

            itens.Add(new ItemPedido
            {
                PizzaId = pizza.Id,
                NomePizza = pizza.Nome,
                Quantidade = item.Quantidade,
                PrecoUnitario = pizza.Preco
            });
        }

        // 2) Grava o pedido no armazenamento PRÓPRIO deste serviço.
        var pedido = repositorio.Adicionar(requisicao.Cliente.Trim(), itens);

        // 3) Envia a comanda à Cozinha. Se ela estiver fora do ar, desfazemos o pedido
        //    para não deixar um pedido que nunca será preparado (uma "compensação" simples).
        try
        {
            await cozinha.EnviarComandaAsync(pedido, cancelamento);
        }
        catch (ServicoIndisponivelException ex)
        {
            repositorio.Remover(pedido.Id);
            return ServicoIndisponivel(ex);
        }

        return CreatedAtAction(nameof(Buscar), new { id = pedido.Id }, pedido);
    }

    private ObjectResult PedidoInvalido(string detalhe) =>
        Problem(title: "Pedido inválido", detail: detalhe, statusCode: StatusCodes.Status400BadRequest);

    private ObjectResult ServicoIndisponivel(ServicoIndisponivelException ex)
    {
        logger.LogWarning(ex, "Falha ao chamar o serviço de {Servico}", ex.Servico);
        return Problem(
            title: $"Serviço de {ex.Servico} indisponível",
            detail: $"{ex.Message} — o pedido não foi registrado.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}
