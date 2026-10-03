using System.Net;
using System.Net.Http.Json;
using TechStore.Produtos.Api.Contracts;

namespace TechStore.Produtos.Api.Tests;

public sealed class ProdutosApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static ProdutoRequest NovoProduto(string sku) =>
        new(sku, "Produto de teste", "Descrição", "Testes", 10.50m, 5);

    [Fact]
    public async Task Health_RetornaOk()
    {
        var resposta = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Listar_RetornaProdutosIniciais()
    {
        var produtos = await _client.GetFromJsonAsync<List<ProdutoResponse>>("/api/produtos");

        Assert.NotNull(produtos);
        Assert.Contains(produtos, p => p.Sku == "EBOOK-AZ-001");
    }

    [Fact]
    public async Task Criar_ComPrecoInvalido_Retorna400()
    {
        var resposta = await _client.PostAsJsonAsync("/api/produtos", NovoProduto("INVALIDO-1") with { Preco = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Criar_SkuDuplicado_Retorna409()
    {
        await _client.PostAsJsonAsync("/api/produtos", NovoProduto("DUP-1"));
        var segunda = await _client.PostAsJsonAsync("/api/produtos", NovoProduto("dup-1"));

        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
    }

    [Fact]
    public async Task CicloCompleto_CriarAtualizarExcluir()
    {
        var criado = await _client.PostAsJsonAsync("/api/produtos", NovoProduto("CICLO-1"));
        Assert.Equal(HttpStatusCode.Created, criado.StatusCode);
        var produto = (await criado.Content.ReadFromJsonAsync<ProdutoResponse>())!;

        var atualizado = await _client.PutAsJsonAsync($"/api/produtos/{produto.Id}", NovoProduto("CICLO-1") with { Preco = 20m });
        Assert.Equal(20m, (await atualizado.Content.ReadFromJsonAsync<ProdutoResponse>())!.Preco);

        var excluido = await _client.DeleteAsync($"/api/produtos/{produto.Id}");
        Assert.Equal(HttpStatusCode.NoContent, excluido.StatusCode);

        var buscado = await _client.GetAsync($"/api/produtos/{produto.Id}");
        Assert.Equal(HttpStatusCode.NotFound, buscado.StatusCode);
    }
}
