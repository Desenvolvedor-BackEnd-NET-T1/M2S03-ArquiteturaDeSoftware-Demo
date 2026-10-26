// Interface do monólito: todas as chamadas vão para a MESMA aplicação que serviu esta página,
// por isso usamos caminhos relativos como "/api/cardapio".

const PROXIMO_STATUS = { Recebido: "EmPreparo", EmPreparo: "NoForno", NoForno: "Pronto", Pronto: "Entregue" };
const ROTULO_STATUS = { Recebido: "Recebido", EmPreparo: "Em preparo", NoForno: "No forno", Pronto: "Pronto", Entregue: "Entregue" };

const moeda = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
let cardapio = [];

async function chamarApi(caminho, opcoes = {}) {
  let resposta;
  try {
    resposta = await fetch(caminho, opcoes);
  } catch {
    throw new Error("Não foi possível conectar à aplicação. Verifique se o monólito está em execução.");
  }
  if (!resposta.ok) {
    throw new Error(await extrairErro(resposta));
  }
  return resposta.json();
}

// Converte uma resposta de erro (ProblemDetails do ASP.NET Core) em texto legível.
async function extrairErro(resposta) {
  try {
    const problema = await resposta.json();
    if (problema.errors) return Object.values(problema.errors).flat().join(" ");
    return problema.detail || problema.title || `Erro ${resposta.status}`;
  } catch {
    return `Erro ${resposta.status}`;
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

async function carregarCardapio() {
  try {
    cardapio = await chamarApi("/api/cardapio");
    esconderMensagem("mensagem-cardapio");
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
  }
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

async function fazerPedido() {
  const botao = document.getElementById("botao-pedido");
  botao.disabled = true;
  try {
    const pedido = await chamarApi("/api/pedidos", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ cliente: document.getElementById("cliente").value, itens: itensSelecionados() })
    });
    mostrarMensagem("mensagem-pedido", `Pedido #${pedido.id} criado com sucesso!`, "sucesso");
    document.querySelectorAll("[data-pizza-id]").forEach(input => (input.value = 0));
    atualizarTotal();
    await carregarPedidos();
  } catch (erro) {
    mostrarMensagem("mensagem-pedido", erro.message);
  } finally {
    botao.disabled = false;
  }
}

async function carregarPedidos() {
  const corpo = document.getElementById("lista-pedidos");
  try {
    const pedidos = await chamarApi("/api/pedidos");
    esconderMensagem("mensagem-pedidos");
    if (pedidos.length === 0) {
      corpo.innerHTML = `<tr><td colspan="6" class="vazio">Nenhum pedido ainda.</td></tr>`;
      return;
    }
    corpo.innerHTML = pedidos.map(pedido => {
      const proximo = PROXIMO_STATUS[pedido.status];
      return `
        <tr>
          <td>${pedido.id}</td>
          <td>${escaparHtml(pedido.cliente)}</td>
          <td>${pedido.itens.map(i => `${i.quantidade}x ${escaparHtml(i.nomePizza)}`).join("<br>")}</td>
          <td>${moeda.format(pedido.total)}</td>
          <td><span class="status ${pedido.status}">${ROTULO_STATUS[pedido.status]}</span></td>
          <td>${proximo ? `<button class="secundario" data-avancar="${pedido.id}" data-status="${proximo}">→ ${ROTULO_STATUS[proximo]}</button>` : ""}</td>
        </tr>`;
    }).join("");
  } catch (erro) {
    mostrarMensagem("mensagem-pedidos", erro.message);
  }
}

async function avancarStatus(id, novoStatus) {
  try {
    await chamarApi(`/api/pedidos/${id}/status`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ status: novoStatus })
    });
    await carregarPedidos();
  } catch (erro) {
    mostrarMensagem("mensagem-pedidos", erro.message);
  }
}

document.getElementById("botao-pedido").addEventListener("click", fazerPedido);
document.getElementById("lista-pedidos").addEventListener("click", evento => {
  const botao = evento.target.closest("[data-avancar]");
  if (botao) avancarStatus(botao.dataset.avancar, botao.dataset.status);
});

carregarCardapio();
carregarPedidos();
setInterval(carregarPedidos, 5000);
