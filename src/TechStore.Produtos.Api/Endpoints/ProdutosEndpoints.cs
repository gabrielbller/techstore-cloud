using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TechStore.Produtos.Api.Contracts;
using TechStore.Produtos.Api.Data;
using TechStore.Produtos.Api.Domain;

namespace TechStore.Produtos.Api.Endpoints;

public static class ProdutosEndpoints
{
    public static void MapProdutosEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/produtos").WithTags("Produtos");

        grupo.MapGet("/", ListarAsync).WithSummary("Lista produtos (filtro opcional por nome ou SKU).");
        grupo.MapGet("/{id:guid}", ObterAsync).WithSummary("Obtém um produto.");
        grupo.MapPost("/", CriarAsync).WithSummary("Cadastra um produto.");
        grupo.MapPut("/{id:guid}", AtualizarAsync).WithSummary("Atualiza um produto.");
        grupo.MapDelete("/{id:guid}", ExcluirAsync).WithSummary("Exclui um produto.");
    }

    private static async Task<Ok<List<ProdutoResponse>>> ListarAsync(ProdutosDbContext db, string? busca)
    {
        var consulta = db.Produtos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            consulta = consulta.Where(p => p.Nome.Contains(busca) || p.Sku.Contains(busca));
        }

        var produtos = await consulta
            .OrderBy(p => p.Nome)
            .Select(p => ProdutoResponse.De(p))
            .ToListAsync();

        return TypedResults.Ok(produtos);
    }

    private static async Task<Results<Ok<ProdutoResponse>, NotFound>> ObterAsync(Guid id, ProdutosDbContext db)
    {
        var produto = await db.Produtos.FindAsync(id);
        return produto is null ? TypedResults.NotFound() : TypedResults.Ok(ProdutoResponse.De(produto));
    }

    private static async Task<Results<Created<ProdutoResponse>, ValidationProblem, Conflict<string>>> CriarAsync(
        ProdutoRequest request, ProdutosDbContext db)
    {
        var erros = request.Validar();
        if (erros.Count > 0)
            return TypedResults.ValidationProblem(erros);

        var produto = new Produto { Id = Guid.NewGuid(), CriadoEm = DateTimeOffset.UtcNow };
        request.AplicarEm(produto);

        if (await db.Produtos.AnyAsync(p => p.Sku == produto.Sku))
            return TypedResults.Conflict($"Já existe um produto com o SKU {produto.Sku}.");

        db.Produtos.Add(produto);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/api/produtos/{produto.Id}", ProdutoResponse.De(produto));
    }

    private static async Task<Results<Ok<ProdutoResponse>, NotFound, ValidationProblem, Conflict<string>>> AtualizarAsync(
        Guid id, ProdutoRequest request, ProdutosDbContext db)
    {
        var erros = request.Validar();
        if (erros.Count > 0)
            return TypedResults.ValidationProblem(erros);

        var produto = await db.Produtos.FindAsync(id);
        if (produto is null)
            return TypedResults.NotFound();

        request.AplicarEm(produto);

        if (await db.Produtos.AnyAsync(p => p.Sku == produto.Sku && p.Id != id))
            return TypedResults.Conflict($"Já existe um produto com o SKU {produto.Sku}.");

        await db.SaveChangesAsync();
        return TypedResults.Ok(ProdutoResponse.De(produto));
    }

    private static async Task<Results<NoContent, NotFound>> ExcluirAsync(Guid id, ProdutosDbContext db)
    {
        var produto = await db.Produtos.FindAsync(id);
        if (produto is null)
            return TypedResults.NotFound();

        db.Produtos.Remove(produto);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}
