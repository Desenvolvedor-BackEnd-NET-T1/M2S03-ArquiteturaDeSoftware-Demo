using Pizzaria.Monolito.Data;
using Pizzaria.Monolito.Models;

namespace Pizzaria.Monolito.Services;

public class CardapioService(BancoEmMemoria banco)
{
    public IReadOnlyList<Pizza> ListarPizzas() => banco.Pizzas;

    public Pizza? BuscarPizza(int id) => banco.Pizzas.FirstOrDefault(pizza => pizza.Id == id);
}
