# Pizzaria — Monólito vs. Microsserviços

Dois exemplos didáticos do mesmo sistema básico de pizzaria (cardápio, pedidos e acompanhamento
do preparo), feitos em **.NET 10 / ASP.NET Core** com HTML, CSS e JavaScript puros:

| Pasta                                          | Arquitetura     | Como acessar             |
|------------------------------------------------|-----------------|--------------------------|
| [`monolito/`](monolito/README.md)              | Monólito        | http://localhost:5050    |
| [`microsservices/`](microsservices/README.md)  | Microsserviços  | http://localhost:5100    |

Os dois podem rodar ao mesmo tempo (as portas não conflitam). Todos os dados ficam em memória e
são perdidos ao reiniciar. Não há dependência de Docker, banco externo ou internet.

Pré-requisito: [SDK do .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).
Para depurar, abra esta pasta no VS Code com a extensão **C#** e use o painel **Executar e Depurar**
(as configurações estão em `.vscode/`). Detalhes no README de cada pasta.
