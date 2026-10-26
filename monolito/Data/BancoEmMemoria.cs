using Pizzaria.Monolito.Models;

namespace Pizzaria.Monolito.Data;

// Simula o banco de dados ÚNICO do monólito: cardápio e pedidos ficam juntos.
// Tudo vive na memória do processo — ao reiniciar a aplicação, os pedidos são perdidos.
public class BancoEmMemoria
{
    // Requisições HTTP chegam em paralelo; o lock evita que duas alterem as listas ao mesmo tempo.
    public object Trava { get; } = new();

    public List<Pizza> Pizzas { get; } =
    [
        new() { Id = 1, Nome = "Margherita", Descricao = "Molho de tomate, mussarela e manjericão", Preco = 42.00m },
        new() { Id = 2, Nome = "Calabresa", Descricao = "Calabresa fatiada, cebola e azeitonas", Preco = 45.00m },
        new() { Id = 3, Nome = "Quatro Queijos", Descricao = "Mussarela, provolone, parmesão e gorgonzola", Preco = 52.00m },
        new() { Id = 4, Nome = "Portuguesa", Descricao = "Presunto, ovos, cebola, ervilha e mussarela", Preco = 49.00m },
        new() { Id = 5, Nome = "Chocolate", Descricao = "Chocolate ao leite com morangos", Preco = 39.00m }
    ];

    public List<Pedido> Pedidos { get; } = [];

    public int ProximoIdPedido { get; set; } = 1;
}
