using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivroCDF.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoVendas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Livros",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataVenda",
                table: "Livros",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Livros",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Telefone",
                table: "Clientes",
                type: "varchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Livros_ClienteId",
                table: "Livros",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Livros_Clientes_ClienteId",
                table: "Livros",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Livros_Clientes_ClienteId",
                table: "Livros");

            migrationBuilder.DropIndex(
                name: "IX_Livros_ClienteId",
                table: "Livros");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Livros");

            migrationBuilder.DropColumn(
                name: "DataVenda",
                table: "Livros");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Livros");

            migrationBuilder.AlterColumn<string>(
                name: "Telefone",
                table: "Clientes",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldMaxLength: 15,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
