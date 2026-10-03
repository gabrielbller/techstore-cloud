using Microsoft.EntityFrameworkCore;
using TechStore.Produtos.Api.Domain;

namespace TechStore.Produtos.Api.Data;

public sealed class ProdutosDbContext(DbContextOptions<ProdutosDbContext> options) : DbContext(options)
{
    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("Produtos");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Sku).HasMaxLength(40).IsRequired();
            entity.HasIndex(p => p.Sku).IsUnique();

            entity.Property(p => p.Nome).HasMaxLength(120).IsRequired();
            entity.Property(p => p.Descricao).HasMaxLength(1000);
            entity.Property(p => p.Categoria).HasMaxLength(60).IsRequired();

            entity.Property(p => p.Preco).HasPrecision(18, 2);

            entity.HasData(DadosIniciais.Produtos);
        });
    }
}
