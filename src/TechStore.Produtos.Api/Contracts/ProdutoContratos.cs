using TechStore.Produtos.Api.Domain;

namespace TechStore.Produtos.Api.Contracts;

public sealed record ProdutoRequest(string Sku, string Nome, string? Descricao, string Categoria, decimal Preco, int Estoque)
{
    public Dictionary<string, string[]> Validar()
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Sku) || Sku.Length > 40)
            erros[nameof(Sku)] = ["SKU é obrigatório (até 40 caracteres)."];
        if (string.IsNullOrWhiteSpace(Nome) || Nome.Length > 120)
            erros[nameof(Nome)] = ["Nome é obrigatório (até 120 caracteres)."];
        if (string.IsNullOrWhiteSpace(Categoria) || Categoria.Length > 60)
            erros[nameof(Categoria)] = ["Categoria é obrigatória (até 60 caracteres)."];
        if (Preco <= 0)
            erros[nameof(Preco)] = ["Preço deve ser maior que zero."];
        if (Estoque < 0)
            erros[nameof(Estoque)] = ["Estoque não pode ser negativo."];

        return erros;
    }

    public void AplicarEm(Produto produto)
    {
        produto.Sku = Sku.Trim().ToUpperInvariant();
        produto.Nome = Nome.Trim();
        produto.Descricao = Descricao?.Trim();
        produto.Categoria = Categoria.Trim();
        produto.Preco = Preco;
        produto.Estoque = Estoque;
    }
}

public sealed record ProdutoResponse(
    Guid Id, string Sku, string Nome, string? Descricao, string Categoria, decimal Preco, int Estoque, DateTimeOffset CriadoEm)
{
    public static ProdutoResponse De(Produto p) =>
        new(p.Id, p.Sku, p.Nome, p.Descricao, p.Categoria, p.Preco, p.Estoque, p.CriadoEm);
}
