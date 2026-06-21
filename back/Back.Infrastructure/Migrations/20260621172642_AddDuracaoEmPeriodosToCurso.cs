using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDuracaoEmPeriodosToCurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DuracaoEmPeriodos",
                table: "Cursos",
                type: "integer",
                nullable: false,
                // Duração padrão de 8 períodos para cursos já existentes.
                defaultValue: 8);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DuracaoEmPeriodos",
                table: "Cursos");
        }
    }
}
