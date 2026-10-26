using Microsoft.AspNetCore.Mvc;
using Pizzaria.Monolito.Models;
using Pizzaria.Monolito.Services;

namespace Pizzaria.Monolito.Controllers;

[ApiController]
[Route("api/cardapio")]
public class CardapioController(CardapioService cardapio) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Pizza>> Listar() => Ok(cardapio.ListarPizzas());

    [HttpGet("{id:int}")]
    public ActionResult<Pizza> Buscar(int id)
    {
        var pizza = cardapio.BuscarPizza(id);
        return pizza is null ? NotFound() : Ok(pizza);
    }
}
