# Pizzaria — Microsserviços

Exemplo didático da mesma pizzaria do [`../monolito`](../monolito/README.md), agora dividida em
**um frontend e três microsserviços independentes**. Cada serviço é um projeto executável próprio,
com sua própria API, sua própria porta e seu próprio armazenamento em memória.

## Componentes e responsabilidades

| Componente   | Pasta                 | Porta | Responsabilidade                                                                 |
|--------------|-----------------------|-------|----------------------------------------------------------------------------------|
| **Frontend** | `frontend/`           | 5100  | Entrega a interface web (HTML/CSS/JS). Não tem regras de negócio nem dados.       |
| **Cardápio** | `servicos/cardapio/`  | 5101  | Dono dos produtos e preços.                                                      |
| **Pedidos**  | `servicos/pedidos/`   | 5102  | Dono dos pedidos (cliente, itens, preço cobrado e total). Orquestra a criação.   |
| **Cozinha**  | `servicos/cozinha/`   | 5103  | Dono do andamento do preparo (status) de cada pedido.                            |

As portas são fixas e definidas na chave `Urls` do `appsettings.json` de cada projeto.

### Princípios aplicados

- **Banco de dados por serviço**: cada serviço tem seu repositório em memória (`Data/`). Nenhum
  serviço lê ou altera os dados de outro diretamente.
- **Comunicação por HTTP**: quando um serviço precisa de algo de outro, ele chama a API HTTP
  dele (veja `servicos/pedidos/Clientes/`). Os projetos **não se referenciam** entre si; o contrato
  entre eles é apenas o JSON trocado.
- **Dados com um único dono**: o pedido não guarda status (isso é da Cozinha) e a Cozinha não
  conhece preços (isso é do Cardápio e de Pedidos).
- **Falhas são visíveis**: se um serviço estiver fora do ar, a interface mostra uma mensagem de erro
  dizendo qual serviço falhou — nada de respostas de sucesso fictícias.

## Estrutura

```
microsservices/
├── Pizzaria.Microsservicos.slnx      # Solução que agrupa os 4 projetos (só para compilar junto)
├── frontend/                         # Pizzaria.Frontend      — porta 5100
│   ├── Program.cs                    #   arquivos estáticos + GET /config (endereços dos serviços)
│   └── wwwroot/                      #   index.html, css/, js/app.js
└── servicos/
    ├── cardapio/                     # Pizzaria.Cardapio.Api  — porta 5101
    │   ├── Controllers/  Data/  Models/
    ├── pedidos/                      # Pizzaria.Pedidos.Api   — porta 5102
    │   ├── Clientes/                 #   clientes HTTP para Cardápio e Cozinha
    │   ├── Controllers/  Data/  Models/
    └── cozinha/                      # Pizzaria.Cozinha.Api   — porta 5103
        ├── Controllers/  Data/  Models/
```

## APIs

**Cardápio — http://localhost:5101**

| Método | Rota                 | Descrição            |
|--------|----------------------|----------------------|
| GET    | `/api/cardapio`      | Lista pizzas e preços |
| GET    | `/api/cardapio/{id}` | Consulta uma pizza   |

**Pedidos — http://localhost:5102**

| Método | Rota                | Descrição                                                    |
|--------|---------------------|--------------------------------------------------------------|
| GET    | `/api/pedidos`      | Lista os pedidos                                             |
| GET    | `/api/pedidos/{id}` | Consulta um pedido                                           |
| POST   | `/api/pedidos`      | Cria um pedido (consulta o Cardápio e envia comanda à Cozinha) |

**Cozinha — http://localhost:5103**

| Método | Rota                             | Descrição                                     |
|--------|----------------------------------|-----------------------------------------------|
| GET    | `/api/comandas`                  | Lista as comandas e seus status               |
| GET    | `/api/comandas/{pedidoId}`       | Consulta a comanda de um pedido               |
| POST   | `/api/comandas`                  | Recebe uma comanda (chamado pelo serviço de Pedidos) |
| PUT    | `/api/comandas/{pedidoId}/status`| Atualiza o status de preparo                  |

Todos os serviços expõem também `GET /health` (usado pela interface para mostrar quem está online).

## Fluxo de um pedido

```
 Navegador (página do frontend :5100)
   │
   │ 1. GET  /api/cardapio ───────────────────────────────▶ Cardápio :5101
   │
   │ 2. POST /api/pedidos ──▶ Pedidos :5102
   │                            │ 2a. GET  /api/cardapio ─▶ Cardápio :5101  (valida pizzas e preços)
   │                            │ 2b. grava o pedido no SEU armazenamento
   │                            │ 2c. POST /api/comandas ─▶ Cozinha  :5103  (envia a comanda)
   │                            ◀── 201 Created (ou 503 se Cardápio/Cozinha falhar)
   │
   │ 3. GET  /api/pedidos  ─▶ Pedidos :5102   ┐ a página junta os dois resultados:
   │    GET  /api/comandas ─▶ Cozinha :5103   ┘ dados do pedido + status de preparo
   │
   │ 4. PUT  /api/comandas/{id}/status ───────────────────▶ Cozinha :5103  (avança o status)
```

1. A página pergunta ao **frontend** onde estão os serviços (`GET /config`) e carrega o cardápio
   direto do serviço de **Cardápio**.
2. Ao clicar em "Fazer pedido", a página chama o serviço de **Pedidos**, que:
   consulta os preços no **Cardápio**, grava o pedido e envia a comanda à **Cozinha**.
   Se a Cozinha não responder, o pedido é desfeito e a página exibe o erro (HTTP 503).
3. A lista de pedidos combina dados de **Pedidos** (itens e total) e da **Cozinha** (status).
   A cada 5 segundos ela é atualizada.
4. O botão "→ próximo status" chama a **Cozinha** para avançar:
   `Recebido` → `EmPreparo` → `NoForno` → `Pronto` → `Entregue`.

> Dica para a aula: abra as ferramentas do desenvolvedor do navegador (F12 → aba **Rede/Network**)
> e observe as requisições indo para portas diferentes.

## Pré-requisitos

- [SDK do .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) (confira com `dotnet --list-sdks`).
- Opcional: [VS Code](https://code.visualstudio.com/) com a extensão **C#** (`ms-dotnettools.csharp`).
  A extensão **C# Dev Kit** é recomendada, mas não obrigatória.
- Um navegador moderno. Acesse sempre por **`http://localhost:5100`** (os serviços liberam CORS
  para `localhost:5100` e `127.0.0.1:5100`).

Não é necessário Docker, banco de dados externo, credenciais nem internet para executar.

## Como compilar

A partir da pasta `microsservices/`, compile tudo de uma vez pela solução:

```bash
dotnet restore Pizzaria.Microsservicos.slnx
dotnet build   Pizzaria.Microsservicos.slnx
```

Ou compile um componente isoladamente (exemplo):

```bash
dotnet build servicos/cardapio
```

## Como executar pelo terminal

Cada componente é um processo independente. Abra **quatro terminais** na pasta `microsservices/`
e execute um comando em cada um:

```bash
dotnet run --project servicos/cardapio    # http://localhost:5101
dotnet run --project servicos/pedidos     # http://localhost:5102
dotnet run --project servicos/cozinha     # http://localhost:5103
dotnet run --project frontend             # http://localhost:5100
```

Depois abra **http://localhost:5100**. Para parar um componente, use `Ctrl+C` no terminal dele.

A ordem não importa: a interface mostra no topo quais serviços estão online e exibe erros claros
para os que ainda não subiram.

### Experimento: derrubando um serviço

- **Pare a Cozinha** (`Ctrl+C` no terminal dela): o cardápio continua funcionando, os pedidos
  continuam listados, mas o status fica "Cozinha indisponível" e novos pedidos são recusados com
  uma mensagem explicando o motivo.
- **Pare o Cardápio**: a página não consegue mostrar as pizzas e o serviço de Pedidos recusa novos
  pedidos, porque não consegue validar os preços.
- **Reinicie só a Cozinha**: os pedidos continuam existindo (estão no serviço de Pedidos), mas
  aparecem como "Sem comanda na cozinha", pois a Cozinha perdeu seu estado em memória.

Compare com o monólito: lá, ou tudo está no ar, ou nada está.

## Como abrir e depurar no VS Code

1. Abra a **pasta raiz do repositório** no VS Code (`File > Open Folder...`) — as configurações
   ficam em `.vscode/` na raiz.
2. Instale a extensão recomendada **C#** quando o VS Code sugerir.
3. Abra o painel **Executar e Depurar** (`Ctrl+Shift+D` / `Cmd+Shift+D`).
4. Escolha uma configuração e pressione **F5**:

| Configuração                                       | O que inicia                                  |
|----------------------------------------------------|-----------------------------------------------|
| **Microsserviços: todos (frontend + 3 serviços)**  | Os 4 processos juntos, com depurador em todos; abre o navegador em :5100 |
| Micro: Cardápio (5101)                             | Só o serviço de Cardápio                      |
| Micro: Pedidos (5102)                              | Só o serviço de Pedidos                       |
| Micro: Cozinha (5103)                              | Só o serviço de Cozinha                       |
| Micro: Frontend (5100)                             | Só o frontend (abre o navegador)              |

Com a configuração composta, cada processo aparece separadamente na barra de depuração e na lista
**Call Stack**; parar a sessão encerra todos. Sugestão de breakpoints para acompanhar um pedido
atravessando os serviços:

- `servicos/pedidos/Controllers/PedidosController.cs` → método `Criar`
- `servicos/cardapio/Controllers/CardapioController.cs` → método `Listar`
- `servicos/cozinha/Controllers/ComandasController.cs` → métodos `Criar` e `AtualizarStatus`

Para executar sem depurador, use **Terminal > Executar Tarefa...** e escolha
**"executar: todos os microsserviços"** (ou `executar: cardapio`, `executar: pedidos`, etc.).
Cada um abre em um terminal próprio.

## Estado volátil em memória

Cada serviço guarda seus dados **apenas na memória do próprio processo**:

- Reiniciar o **Cardápio** recria os dados iniciais (5 pizzas).
- Reiniciar **Pedidos** apaga todos os pedidos (e a numeração volta para 1).
- Reiniciar a **Cozinha** apaga todas as comandas e seus status.

Como os estados são separados, reiniciar um serviço não apaga os dados dos outros — o que pode
deixá-los temporariamente inconsistentes (por exemplo, um pedido sem comanda). Esse é um dos
desafios reais de sistemas distribuídos que o exemplo ajuda a discutir.
