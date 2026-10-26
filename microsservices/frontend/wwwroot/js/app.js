// Interface dos microsserviços: cada operação chama a API do serviço RESPONSÁVEL por ela.
//   Cardápio (5101) -> lista de pizzas e preços
//   Pedidos  (5102) -> criar e listar pedidos
//   Cozinha  (5103) -> status de preparo e avanço do status
// Abra as ferramentas do desenvolvedor (aba Rede/Network) para ver as chamadas a cada porta.

const PROXIMO_STATUS = { Recebido: "EmPreparo", EmPreparo: "NoForno", NoForno: "Pronto", Pronto: "Entregue" };
const ROTULO_STATUS = { Recebido: "Recebido", EmPreparo: "Em preparo", NoForno: "No forno", Pronto: "Pronto", Entregue: "Entregue" };

const moeda = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
let servicos = {};
let cardapio = [];

// Faz uma chamada HTTP a um serviço e transforma qualquer falha em uma mensagem clara.
async function chamarServico(nomeServico, urlBase, caminho, opcoes = {}) {
  let resposta;
  try {
    resposta = await fetch(urlBase + caminho, opcoes);
  } catch {
    throw new Error(`Não foi possível conectar ao serviço de ${nomeServico} (${urlBase}). Verifique se ele está em execução.`);
  }
  if (!resposta.ok) {
    throw new Error(`Serviço de ${nomeServico} respondeu com erro ${resposta.status}: ${await extrairErro(resposta)}`);
  }
  return resposta.json();
}

// Converte uma resposta de erro (ProblemDetails do ASP.NET Core) em texto legível.
async function extrairErro(resposta) {
  try {
    const problema = await resposta.json();
    if (problema.errors) return Object.values(problema.errors).flat().join(" ");
    return [problema.title, problema.detail].filter(Boolean).join(" — ") || resposta.statusText;
  } catch {
    return resposta.statusText || "erro desconhecido";
  }
}

function mostrarMensagem(id, texto, tipo = "erro") {
  const elemento = document.getElementById(id);
  elemento.textContent = texto;
  elemento.className = `mensagem ${tipo}`;
}

function esconderMensagem(id) {
  document.getElementById(id).className = "mensagem oculto";
}

function escaparHtml(texto) {
  const div = document.createElement("div");
  div.textContent = texto;
  return div.innerHTML;
}

// ---------- Saúde dos serviços (endpoint /health de cada um) ----------
async function verificarSaude() {
  const lista = [["cardapio", servicos.cardapioUrl], ["pedidos", servicos.pedidosUrl], ["cozinha", servicos.cozinhaUrl]];
  await Promise.all(lista.map(async ([nome, url]) => {
    const indicador = document.getElementById(`saude-${nome}`);
    let online = false;
    try {
      online = (await fetch(`${url}/health`)).ok;
    } catch { /* serviço fora do ar */ }
    indicador.classList.toggle("online", online);
    indicador.classList.toggle("offline", !online);
    indicador.title = online ? "Online" : "Fora do ar";
  }));
}

// ---------- Cardápio ----------
async function carregarCardapio() {
  const botaoRecarregar = document.getElementById("botao-recarregar-cardapio");
  try {
    cardapio = await chamarServico("Cardápio", servicos.cardapioUrl, "/api/cardapio");
    esconderMensagem("mensagem-cardapio");
    botaoRecarregar.classList.add("oculto");
    document.getElementById("lista-cardapio").innerHTML = cardapio.map(pizza => `
      <div class="pizza">
        <div>
          <div class="nome">${escaparHtml(pizza.nome)}</div>
          <div class="descricao">${escaparHtml(pizza.descricao)}</div>
        </div>
        <span class="preco">${moeda.format(pizza.preco)}</span>
        <input type="number" min="0" value="0" data-pizza-id="${pizza.id}" aria-label="Quantidade de ${escaparHtml(pizza.nome)}">
      </div>`).join("");
    document.querySelectorAll("[data-pizza-id]").forEach(input => input.addEventListener("input", atualizarTotal));
  } catch (erro) {
    document.getElementById("lista-cardapio").innerHTML = "";
    mostrarMensagem("mensagem-cardapio", erro.message);
    botaoRecarregar.classList.remove("oculto");
  }
  atualizarTotal();
}

function itensSelecionados() {
  return [...document.querySelectorAll("[data-pizza-id]")]
    .map(input => ({ pizzaId: Number(input.dataset.pizzaId), quantidade: Number(input.value) }))
    .filter(item => item.quantidade > 0);
}

function atualizarTotal() {
  const total = itensSelecionados().reduce((soma, item) => {
    const pizza = cardapio.find(p => p.id === item.pizzaId);
    return soma + (pizza ? pizza.preco * item.quantidade : 0);
  }, 0);
  document.getElementById("total-pedido").textContent = moeda.format(total);
}

// ---------- Pedidos ----------
async function fazerPedido() {
  const botao = document.getElementById("botao-pedido");
  botao.disabled = true;
  try {
    // O frontend só fala com o serviço de Pedidos; é ELE quem consulta o Cardápio e avisa a Cozinha.
    const pedido = await chamarServico("Pedidos", servicos.pedidosUrl, "/api/pedidos", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ cliente: document.getElementById("cliente").value, itens: itensSelecionados() })
    });
    mostrarMensagem("mensagem-pedido", `Pedido #${pedido.id} criado e enviado para a cozinha!`, "sucesso");
    document.querySelectorAll("[data-pizza-id]").forEach(input => (input.value = 0));
    atualizarTotal();
    await carregarPedidos();
  } catch (erro) {
    mostrarMensagem("mensagem-pedido", erro.message);
  } finally {
    botao.disabled = false;
  }
}

// Junta os dados de dois serviços: pedidos (Pedidos) + status de preparo (Cozinha).
// Se a Cozinha cair, os pedidos continuam visíveis, mas o status aparece como indisponível.
async function carregarPedidos() {
  const corpo = document.getElementById("lista-pedidos");
  const [resultadoPedidos, resultadoComandas] = await Promise.allSettled([
    chamarServico("Pedidos", servicos.pedidosUrl, "/api/pedidos"),
    chamarServico("Cozinha", servicos.cozinhaUrl, "/api/comandas")
  ]);

  if (resultadoComandas.status === "rejected") {
    mostrarMensagem("mensagem-cozinha", resultadoComandas.reason.message);
  } else {
    esconderMensagem("mensagem-cozinha");
  }

  if (resultadoPedidos.status === "rejected") {
    mostrarMensagem("mensagem-pedidos", resultadoPedidos.reason.message);
    corpo.innerHTML = `<tr><td colspan="6" class="vazio">Pedidos indisponíveis.</td></tr>`;
    return;
  }
  esconderMensagem("mensagem-pedidos");

  const pedidos = resultadoPedidos.value;
  const comandas = resultadoComandas.status === "fulfilled" ? resultadoComandas.value : null;

  if (pedidos.length === 0) {
    corpo.innerHTML = `<tr><td colspan="6" class="vazio">Nenhum pedido ainda.</td></tr>`;
    return;
  }

  corpo.innerHTML = pedidos.map(pedido => {
    const comanda = comandas?.find(c => c.pedidoId === pedido.id);
    let colunaStatus;
    let colunaAcao = "";
    if (!comandas) {
      colunaStatus = `<span class="status Indisponivel">Cozinha indisponível</span>`;
    } else if (!comanda) {
      // Ex.: a Cozinha foi reiniciada e perdeu seu estado em memória, mas o serviço de Pedidos não.
      colunaStatus = `<span class="status Indisponivel">Sem comanda na cozinha</span>`;
    } else {
      colunaStatus = `<span class="status ${comanda.status}">${ROTULO_STATUS[comanda.status]}</span>`;
      const proximo = PROXIMO_STATUS[comanda.status];
      if (proximo) {
        colunaAcao = `<button class="secundario" data-avancar="${pedido.id}" data-status="${proximo}">→ ${ROTULO_STATUS[proximo]}</button>`;
      }
    }
    return `
      <tr>
        <td>${pedido.id}</td>
        <td>${escaparHtml(pedido.cliente)}</td>
        <td>${pedido.itens.map(i => `${i.quantidade}x ${escaparHtml(i.nomePizza)}`).join("<br>")}</td>
        <td>${moeda.format(pedido.total)}</td>
        <td>${colunaStatus}</td>
        <td>${colunaAcao}</td>
      </tr>`;
  }).join("");
}

// ---------- Cozinha ----------
async function avancarStatus(pedidoId, novoStatus) {
  try {
    await chamarServico("Cozinha", servicos.cozinhaUrl, `/api/comandas/${pedidoId}/status`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ status: novoStatus })
    });
    await carregarPedidos();
  } catch (erro) {
    mostrarMensagem("mensagem-cozinha", erro.message);
  }
}

// ---------- Inicialização ----------
async function iniciar() {
  try {
    servicos = await (await fetch("/config")).json();
  } catch {
    mostrarMensagem("mensagem-cardapio", "Não foi possível carregar /config do frontend.");
    return;
  }

  document.getElementById("botao-pedido").addEventListener("click", fazerPedido);
  document.getElementById("botao-recarregar-cardapio").addEventListener("click", carregarCardapio);
  document.getElementById("lista-pedidos").addEventListener("click", evento => {
    const botao = evento.target.closest("[data-avancar]");
    if (botao) avancarStatus(botao.dataset.avancar, botao.dataset.status);
  });

  await Promise.all([verificarSaude(), carregarCardapio(), carregarPedidos()]);
  setInterval(() => { verificarSaude(); carregarPedidos(); }, 5000);
}

iniciar();
