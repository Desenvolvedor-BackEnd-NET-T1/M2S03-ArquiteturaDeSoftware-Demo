using Microsoft.AspNetCore.Mvc;
using Pizzaria.Monolito.Models;
using Pizzaria.Monolito.Services;

namespace Pizzaria.Monolito.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController(PedidoService pedidos) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Pedido>> Listar() => Ok(pedidos.ListarPedidos());

    [HttpGet("{id:int}")]
    public ActionResult<Pedido> Buscar(int id)
    {
        var pedido = pedidos.BuscarPedido(id);
        return pedido is null ? NotFound() : Ok(pedido);
    }

    [HttpPost]
    public ActionResult<Pedido> Criar(NovoPedidoRequest requisicao)
    {
        var (pedido, erro) = pedidos.CriarPedido(requisicao);
        if (pedido is null)
            return Problem(title: "Pedido inválido", detail: erro, statusCode: StatusCodes.Status400BadRequest);

        return CreatedAtAction(nameof(Buscar), new { id = pedido.Id }, pedido);
    }

    [HttpPut("{id:int}/status")]
    public ActionResult<Pedido> AtualizarStatus(int id, AtualizarStatusRequest requisicao)
    {
        var pedido = pedidos.AtualizarStatus(id, requisicao.Status);
        return pedido is null ? NotFound() : Ok(pedido);
    }
}
