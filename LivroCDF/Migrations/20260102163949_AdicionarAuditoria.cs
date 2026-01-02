using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivroCDF.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DataSaida",
                table: "Exemplares",
                newName: "DataVenda");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataUltimaAtualizacao",
                table: "Exemplares",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataUltimaAtualizacao",
                table: "Exemplares");

            migrationBuilder.RenameColumn(
                name: "DataVenda",
                table: "Exemplares",
                newName: "DataSaida");
        }
    }
}
