using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clientes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaIndiceCriadoEm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Clientes_CriadoEm",
                table: "Clientes",
                column: "CriadoEm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_CriadoEm",
                table: "Clientes");
        }
    }
}
