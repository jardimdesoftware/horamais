using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjusteAlunoFinalizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JaBaixadoHoras",
                table: "Alunos",
                newName: "JaBaixadoHorasComplementares");

            migrationBuilder.AddColumn<bool>(
                name: "JaBaixadoHorasExtensao",
                table: "Alunos",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JaBaixadoHorasExtensao",
                table: "Alunos");

            migrationBuilder.RenameColumn(
                name: "JaBaixadoHorasComplementares",
                table: "Alunos",
                newName: "JaBaixadoHoras");
        }
    }
}
