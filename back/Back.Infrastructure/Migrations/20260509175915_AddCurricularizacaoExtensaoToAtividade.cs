using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurricularizacaoExtensaoToAtividade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HorasCurricularizacaoExtensao",
                table: "Atividades",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PossuiCurricularizacaoExtensao",
                table: "Atividades",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HorasCurricularizacaoExtensao",
                table: "Atividades");

            migrationBuilder.DropColumn(
                name: "PossuiCurricularizacaoExtensao",
                table: "Atividades");
        }
    }
}
