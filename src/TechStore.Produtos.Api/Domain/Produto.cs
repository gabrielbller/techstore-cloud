namespace TechStore.Produtos.Api.Domain;

/// <summary>Produto digital da TechStore Cloud (e-book, curso, licença, assinatura...).</summary>
public sealed class Produto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
}
