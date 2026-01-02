using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivroCDF.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoClienteNoExemplar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Exemplares",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exemplares_ClienteId",
                table: "Exemplares",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exemplares_Clientes_ClienteId",
                table: "Exemplares",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exemplares_Clientes_ClienteId",
                table: "Exemplares");

            migrationBuilder.DropIndex(
                name: "IX_Exemplares_ClienteId",
                table: "Exemplares");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Exemplares");
        }
    }
}
