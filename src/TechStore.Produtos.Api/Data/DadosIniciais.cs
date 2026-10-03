using TechStore.Produtos.Api.Domain;

namespace TechStore.Produtos.Api.Data;

/// <summary>Produtos de exemplo inseridos pela migration inicial (ids e datas fixos).</summary>
internal static class DadosIniciais
{
    private static readonly DateTimeOffset DataCarga = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    public static readonly Produto[] Produtos =
    [
        new()
        {
            Id = Guid.Parse("6f1c2a10-0001-4c3e-9a51-000000000001"),
            Sku = "EBOOK-AZ-001",
            Nome = "E-book: Arquitetura de Microsserviços na Azure",
            Descricao = "Guia prático com padrões de projeto, observabilidade e segurança.",
            Categoria = "E-books",
            Preco = 59.90m,
            Estoque = 9999,
            CriadoEm = DataCarga
        },
        new()
        {
            Id = Guid.Parse("6f1c2a10-0002-4c3e-9a51-000000000002"),
            Sku = "CURSO-NET-010",
            Nome = "Curso online: APIs REST com .NET",
            Descricao = "40 horas de conteúdo em vídeo, com certificado.",
            Categoria = "Cursos",
            Preco = 349.00m,
            Estoque = 500,
            CriadoEm = DataCarga
        },
        new()
        {
            Id = Guid.Parse("6f1c2a10-0003-4c3e-9a51-000000000003"),
            Sku = "LIC-IDE-PRO-1A",
            Nome = "Licença anual: IDE Pro",
            Descricao = "Licença individual de 12 meses, com atualizações.",
            Categoria = "Licenças de software",
            Preco = 899.00m,
            Estoque = 120,
            CriadoEm = DataCarga
        },
        new()
        {
            Id = Guid.Parse("6f1c2a10-0004-4c3e-9a51-000000000004"),
            Sku = "GIFT-100",
            Nome = "Gift card digital R$ 100",
            Descricao = "Crédito para uso em toda a loja, válido por 12 meses.",
            Categoria = "Gift cards",
            Preco = 100.00m,
            Estoque = 1000,
            CriadoEm = DataCarga
        },
        new()
        {
            Id = Guid.Parse("6f1c2a10-0005-4c3e-9a51-000000000005"),
            Sku = "ASSIN-CLOUD-M",
            Nome = "Assinatura mensal: Laboratórios Cloud",
            Descricao = "Acesso a laboratórios guiados de Azure, renovação mensal.",
            Categoria = "Assinaturas",
            Preco = 79.90m,
            Estoque = 300,
            CriadoEm = DataCarga
        }
    ];
}
