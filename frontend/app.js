"use strict";

// URL da API definida em config.js (gerado no deploy).
const API = (window.TECHSTORE_CONFIG?.apiBaseUrl ?? "").replace(/\/$/, "");

const $ = (seletor) => document.querySelector(seletor);
const moeda = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
let editandoId = null;

function avisar(mensagem) {
  const aviso = $("#aviso");
  aviso.textContent = mensagem;
  aviso.hidden = false;
  clearTimeout(avisar.timer);
  avisar.timer = setTimeout(() => { aviso.hidden = true; }, 3000);
}

async function chamarApi(caminho, opcoes = {}) {
  const resposta = await fetch(`${API}${caminho}`, {
    ...opcoes,
    headers: opcoes.body ? { "Content-Type": "application/json" } : {}
  });
  if (resposta.status === 204) return null;

  const corpo = await resposta.json().catch(() => null);
  if (!resposta.ok) {
    const erros = corpo?.errors ? Object.values(corpo.errors).flat().join("\n") : "";
    throw new Error(erros || (typeof corpo === "string" ? corpo : corpo?.title) || `Erro HTTP ${resposta.status}`);
  }
  return corpo;
}

async function verificarApi() {
  const status = $("#status-api");
  try {
    const resposta = await fetch(`${API}/health`);
    status.textContent = resposta.ok ? "API e banco online" : "API com falha";
    status.className = `status ${resposta.ok ? "status--ok" : "status--erro"}`;
  } catch {
    status.textContent = "API indisponível";
    status.className = "status status--erro";
  }
}

function celula(texto, classe) {
  const td = document.createElement("td");
  if (classe) td.className = classe;
  td.textContent = texto; // textContent evita injeção de HTML (XSS)
  return td;
}

function botao(texto, classe, acao) {
  const b = document.createElement("button");
  b.type = "button";
  b.className = classe;
  b.textContent = texto;
  b.addEventListener("click", acao);
  return b;
}

function linhaProduto(produto) {
  const tr = document.createElement("tr");

  const nome = celula(produto.nome);
  if (produto.descricao) {
    const descricao = document.createElement("span");
    descricao.className = "descricao";
    descricao.textContent = produto.descricao;
    nome.append(descricao);
  }

  const acoes = celula("", "acoes");
  acoes.append(
    botao("Editar", "botao botao--link", () => abrirFormulario(produto)),
    botao("Excluir", "botao botao--link botao--perigo", () => excluirProduto(produto)));

  tr.append(
    celula(produto.sku, "sku"),
    nome,
    celula(produto.categoria),
    celula(moeda.format(produto.preco), "num"),
    celula(produto.estoque.toLocaleString("pt-BR"), "num"),
    acoes);
  return tr;
}

async function carregarProdutos() {
  const busca = $("#busca").value.trim();
  try {
    const produtos = await chamarApi(`/api/produtos${busca ? `?busca=${encodeURIComponent(busca)}` : ""}`);
    $("#lista-produtos").replaceChildren(...produtos.map(linhaProduto));
    $("#vazio").hidden = produtos.length > 0;
    $("#resumo").textContent = `${produtos.length} produto(s)`;
  } catch (erro) {
    $("#lista-produtos").replaceChildren();
    $("#vazio").hidden = false;
    $("#vazio").textContent = `Não foi possível carregar os produtos: ${erro.message}`;
  }
}

function abrirFormulario(produto) {
  const form = $("#form-produto");
  form.reset();
  $("#form-erro").hidden = true;
  editandoId = produto?.id ?? null;
  $("#dlg-titulo").textContent = produto ? "Editar produto" : "Novo produto";

  if (produto) {
    form.sku.value = produto.sku;
    form.nome.value = produto.nome;
    form.descricao.value = produto.descricao ?? "";
    form.categoria.value = produto.categoria;
    form.preco.value = produto.preco;
    form.estoque.value = produto.estoque;
  }
  $("#dlg-produto").showModal();
}

async function salvarProduto(evento) {
  if (evento.submitter?.value !== "salvar") return;
  evento.preventDefault();
  const form = evento.target;

  const dados = {
    sku: form.sku.value,
    nome: form.nome.value,
    descricao: form.descricao.value || null,
    categoria: form.categoria.value,
    preco: Number(form.preco.value),
    estoque: Number.parseInt(form.estoque.value, 10)
  };

  try {
    await chamarApi(editandoId ? `/api/produtos/${editandoId}` : "/api/produtos", {
      method: editandoId ? "PUT" : "POST",
      body: JSON.stringify(dados)
    });
    $("#dlg-produto").close();
    avisar(editandoId ? "Produto atualizado." : "Produto cadastrado.");
    await carregarProdutos();
  } catch (erro) {
    $("#form-erro").textContent = erro.message;
    $("#form-erro").hidden = false;
  }
}

async function excluirProduto(produto) {
  if (!confirm(`Excluir "${produto.nome}"?`)) return;
  try {
    await chamarApi(`/api/produtos/${produto.id}`, { method: "DELETE" });
    avisar("Produto excluído.");
    await carregarProdutos();
  } catch (erro) {
    avisar(erro.message);
  }
}

let debounce;
$("#busca").addEventListener("input", () => {
  clearTimeout(debounce);
  debounce = setTimeout(carregarProdutos, 300);
});
$("#btn-novo").addEventListener("click", () => abrirFormulario(null));
$("#form-produto").addEventListener("submit", salvarProduto);

verificarApi();
carregarProdutos();
