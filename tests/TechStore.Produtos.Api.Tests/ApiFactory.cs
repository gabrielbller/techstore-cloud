using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TechStore.Produtos.Api.Data;

namespace TechStore.Produtos.Api.Tests;

/// <summary>Sobe a API em memória, trocando o Azure SQL por um banco InMemory.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _nomeBanco = $"produtos-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Database:MigrateOnStartup", "false");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ProdutosDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ProdutosDbContext>>();
            services.AddDbContext<ProdutosDbContext>(options => options.UseInMemoryDatabase(_nomeBanco));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ProdutosDbContext>().Database.EnsureCreated();
        return host;
    }
}
