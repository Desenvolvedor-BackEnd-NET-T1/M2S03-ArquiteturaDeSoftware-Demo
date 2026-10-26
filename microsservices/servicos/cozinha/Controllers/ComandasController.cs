using Microsoft.AspNetCore.Mvc;
using Pizzaria.Cozinha.Api.Data;
using Pizzaria.Cozinha.Api.Models;

namespace Pizzaria.Cozinha.Api.Controllers;

[ApiController]
[Route("api/comandas")]
public class ComandasController(ComandaRepositorio repositorio) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Comanda>> Listar() => Ok(repositorio.Listar());

    [HttpGet("{pedidoId:int}")]
    public ActionResult<Comanda> Buscar(int pedidoId)
    {
        var comanda = repositorio.Buscar(pedidoId);
        return comanda is null ? NotFound() : Ok(comanda);
    }

    // Chamado pelo serviço de Pedidos quando um novo pedido é criado.
    [HttpPost]
    public ActionResult<Comanda> Criar(NovaComandaRequest requisicao)
    {
        if (requisicao.PedidoId <= 0 || requisicao.Itens.Count == 0)
            return Problem(title: "Comanda inválida", detail: "Informe o pedido e ao menos um item.", statusCode: StatusCodes.Status400BadRequest);

        var agora = DateTime.Now;
        var comanda = new Comanda
        {
            PedidoId = requisicao.PedidoId,
            Cliente = requisicao.Cliente,
            Itens = requisicao.Itens,
            Status = StatusPreparo.Recebido,
            RecebidaEm = agora,
            AtualizadaEm = agora
        };

        if (!repositorio.Adicionar(comanda))
            return Problem(title: "Comanda duplicada", detail: $"Já existe comanda para o pedido {requisicao.PedidoId}.", statusCode: StatusCodes.Status409Conflict);

        return CreatedAtAction(nameof(Buscar), new { pedidoId = comanda.PedidoId }, comanda);
    }

    [HttpPut("{pedidoId:int}/status")]
    public ActionResult<Comanda> AtualizarStatus(int pedidoId, AtualizarStatusRequest requisicao)
    {
        var comanda = repositorio.AtualizarStatus(pedidoId, requisicao.Status);
        return comanda is null ? NotFound() : Ok(comanda);
    }
}
