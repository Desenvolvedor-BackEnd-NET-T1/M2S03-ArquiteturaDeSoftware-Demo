# Pizzaria — Monólito

Exemplo didático de uma pizzaria construída como **monólito**: uma única aplicação ASP.NET Core
que contém a interface web, a API e todas as regras de negócio.

## O que caracteriza o monólito neste exemplo

- **Um único projeto e um único processo** (`Pizzaria.Monolito.csproj`), iniciado com um único `dotnet run`.
- **A interface web é servida pela própria aplicação** (pasta `wwwroot/`) — a página e a API têm o mesmo endereço.
- **Um único "banco de dados"** (`Data/BancoEmMemoria.cs`) guarda o cardápio e os pedidos juntos.
- **As funcionalidades se comunicam por chamadas de método**: ao criar um pedido, o `PedidoService`
  chama o `CardapioService` diretamente, no mesmo processo, sem rede envolvida.
- **Um único deploy**: qualquer alteração (no cardápio, nos pedidos ou na tela) exige publicar a aplicação inteira.

Compare com a pasta [`../microsservices`](../microsservices/README.md), em que essas mesmas
responsabilidades foram separadas em serviços independentes.

## Estrutura

```
monolito/
├── Pizzaria.Monolito.csproj
├── Program.cs                  # Configuração da aplicação (API + arquivos estáticos)
├── appsettings.json            # Define a porta: http://localhost:5050
├── Models/                     # Pizza, Pedido, StatusPedido, requisições
├── Data/BancoEmMemoria.cs      # "Banco" único em memória, com dados iniciais do cardápio
├── Services/                   # Regras de negócio (CardapioService, PedidoService)
├── Controllers/                # Endpoints HTTP da API
└── wwwroot/                    # Interface web (HTML, CSS e JavaScript puro)
```

## API

| Método | Rota                       | Descrição                                   |
|--------|----------------------------|---------------------------------------------|
| GET    | `/api/cardapio`            | Lista as pizzas e preços                    |
| GET    | `/api/cardapio/{id}`       | Consulta uma pizza                          |
| GET    | `/api/pedidos`             | Lista os pedidos (mais recentes primeiro)   |
| GET    | `/api/pedidos/{id}`        | Consulta um pedido                          |
| POST   | `/api/pedidos`             | Cria um pedido                              |
| PUT    | `/api/pedidos/{id}/status` | Atualiza o status do pedido                 |

Status possíveis: `Recebido` → `EmPreparo` → `NoForno` → `Pronto` → `Entregue`.

Exemplo de criação de pedido:

```bash
curl -X POST http://localhost:5050/api/pedidos \
  -H "Content-Type: application/json" \
  -d '{"cliente":"Ana","itens":[{"pizzaId":1,"quantidade":2}]}'
```

## Pré-requisitos

- [SDK do .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) (confira com `dotnet --list-sdks`; deve aparecer uma versão `10.x`).
- Opcional: [VS Code](https://code.visualstudio.com/) com a extensão **C#** (`ms-dotnettools.csharp`).
  A extensão **C# Dev Kit** é recomendada, mas não obrigatória.

Não é necessário Docker, banco de dados externo, credenciais nem internet para executar.

## Como executar pelo terminal

A partir da pasta `monolito/`:

```bash
dotnet restore     # baixa/prepara as dependências (feito automaticamente pelo build)
dotnet build       # compila
dotnet run         # executa
```

Abra **http://localhost:5050** no navegador. Para parar, pressione `Ctrl+C` no terminal.

## Como abrir e depurar no VS Code

1. Abra a **pasta raiz do repositório** no VS Code (`File > Open Folder...`), não apenas a pasta `monolito/` —
   as configurações de depuração ficam em `.vscode/` na raiz.
2. Instale a extensão recomendada **C#** quando o VS Code sugerir (ou pela aba Extensões).
3. Abra o painel **Executar e Depurar** (`Ctrl+Shift+D` / `Cmd+Shift+D`).
4. Selecione **"Monólito (5050)"** e pressione **F5**.
   O projeto é compilado, a aplicação inicia e o navegador abre em http://localhost:5050.
5. Coloque um breakpoint, por exemplo em `Services/PedidoService.cs` no método `CriarPedido`,
   e faça um pedido pela página para ver a execução parar ali.

Também é possível executar sem depurador em **Terminal > Executar Tarefa... > "executar: monolito"**.

## Dados voláteis

Os dados ficam **apenas na memória** da aplicação. O cardápio é recriado a cada inicialização,
mas **todos os pedidos são perdidos quando a aplicação é reiniciada**. Isso é intencional, para
manter o exemplo simples e sem dependências externas.
