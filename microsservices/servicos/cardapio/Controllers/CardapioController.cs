using Microsoft.AspNetCore.Mvc;
using Pizzaria.Cardapio.Api.Data;
using Pizzaria.Cardapio.Api.Models;

namespace Pizzaria.Cardapio.Api.Controllers;

[ApiController]
[Route("api/cardapio")]
public class CardapioController(CardapioRepositorio repositorio) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Pizza>> Listar() => Ok(repositorio.Listar());

    [HttpGet("{id:int}")]
    public ActionResult<Pizza> Buscar(int id)
    {
        var pizza = repositorio.Buscar(id);
        return pizza is null ? NotFound() : Ok(pizza);
    }
}
