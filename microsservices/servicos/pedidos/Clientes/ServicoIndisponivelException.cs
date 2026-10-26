namespace Pizzaria.Pedidos.Api.Clientes;

// Representa uma falha de comunicação com outro microsserviço (fora do ar, lento ou com erro).
public class ServicoIndisponivelException(string servico, string mensagem, Exception? causa = null)
    : Exception(mensagem, causa)
{
    public string Servico { get; } = servico;
}
