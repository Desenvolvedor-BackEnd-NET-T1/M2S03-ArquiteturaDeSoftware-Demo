using Pizzaria.Cardapio.Api.Models;

namespace Pizzaria.Cardapio.Api.Data;

// Armazenamento em memória exclusivo deste serviço.
// Nenhum outro serviço acessa esta lista: quem precisar do cardápio chama a API HTTP.
public class CardapioRepositorio
{
    private readonly List<Pizza> _pizzas =
    [
        new() { Id = 1, Nome = "Margherita", Descricao = "Molho de tomate, mussarela e manjericão", Preco = 42.00m },
        new() { Id = 2, Nome = "Calabresa", Descricao = "Calabresa fatiada, cebola e azeitonas", Preco = 45.00m },
        new() { Id = 3, Nome = "Quatro Queijos", Descricao = "Mussarela, provolone, parmesão e gorgonzola", Preco = 52.00m },
        new() { Id = 4, Nome = "Portuguesa", Descricao = "Presunto, ovos, cebola, ervilha e mussarela", Preco = 49.00m },
        new() { Id = 5, Nome = "Chocolate", Descricao = "Chocolate ao leite com morangos", Preco = 39.00m }
    ];

    public IReadOnlyList<Pizza> Listar() => _pizzas;

    public Pizza? Buscar(int id) => _pizzas.FirstOrDefault(pizza => pizza.Id == id);
}
