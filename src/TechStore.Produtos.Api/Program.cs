using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TechStore.Produtos.Api.Data;
using TechStore.Produtos.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Telemetria (requisições, dependências SQL, exceções e logs) enviada ao Application Insights.
var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}

// No App Service, a connection string é uma Key Vault reference resolvida com a Managed Identity.
builder.Services.AddDbContext<ProdutosDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ProdutosDb"),
        sql => sql.EnableRetryOnFailure()));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProdutosDbContext>("azure-sql");

// O front-end (Blob Storage static website) roda em outra origem.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("TechStore Cloud - API de Produtos"));
app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

app.MapHealthChecks("/health");
app.MapProdutosEndpoints();

// Cria/atualiza as tabelas no Azure SQL na inicialização.
if (app.Configuration.GetValue("Database:MigrateOnStartup", true))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<ProdutosDbContext>().Database.MigrateAsync();
}

app.Run();

public partial class Program;
