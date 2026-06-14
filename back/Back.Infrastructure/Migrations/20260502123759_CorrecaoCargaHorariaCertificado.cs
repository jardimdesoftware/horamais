using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrecaoCargaHorariaCertificado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CargaHorariaCorrigida",
                table: "Certificados",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CargaHorariaOriginal",
                table: "Certificados",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CargaHorariaCorrigida",
                table: "Certificados");

            migrationBuilder.DropColumn(
                name: "CargaHorariaOriginal",
                table: "Certificados");
        }
    }
}
