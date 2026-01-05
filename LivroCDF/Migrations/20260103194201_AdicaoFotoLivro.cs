using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivroCDF.Migrations
{
    /// <inheritdoc />
    public partial class AdicaoFotoLivro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoCaminho",
                table: "Livros",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoCaminho",
                table: "Livros");
        }
    }
}
