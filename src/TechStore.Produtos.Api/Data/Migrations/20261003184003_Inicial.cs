using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechStore.Produtos.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Categoria = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estoque = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Produtos",
                columns: new[] { "Id", "Categoria", "CriadoEm", "Descricao", "Estoque", "Nome", "Preco", "Sku" },
                values: new object[,]
                {
                    { new Guid("6f1c2a10-0001-4c3e-9a51-000000000001"), "E-books", new DateTimeOffset(new DateTime(2026, 10, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Guia prático com padrões de projeto, observabilidade e segurança.", 9999, "E-book: Arquitetura de Microsserviços na Azure", 59.90m, "EBOOK-AZ-001" },
                    { new Guid("6f1c2a10-0002-4c3e-9a51-000000000002"), "Cursos", new DateTimeOffset(new DateTime(2026, 10, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "40 horas de conteúdo em vídeo, com certificado.", 500, "Curso online: APIs REST com .NET", 349.00m, "CURSO-NET-010" },
                    { new Guid("6f1c2a10-0003-4c3e-9a51-000000000003"), "Licenças de software", new DateTimeOffset(new DateTime(2026, 10, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Licença individual de 12 meses, com atualizações.", 120, "Licença anual: IDE Pro", 899.00m, "LIC-IDE-PRO-1A" },
                    { new Guid("6f1c2a10-0004-4c3e-9a51-000000000004"), "Gift cards", new DateTimeOffset(new DateTime(2026, 10, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Crédito para uso em toda a loja, válido por 12 meses.", 1000, "Gift card digital R$ 100", 100.00m, "GIFT-100" },
                    { new Guid("6f1c2a10-0005-4c3e-9a51-000000000005"), "Assinaturas", new DateTimeOffset(new DateTime(2026, 10, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Acesso a laboratórios guiados de Azure, renovação mensal.", 300, "Assinatura mensal: Laboratórios Cloud", 79.90m, "ASSIN-CLOUD-M" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_Sku",
                table: "Produtos",
                column: "Sku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Produtos");
        }
    }
}
