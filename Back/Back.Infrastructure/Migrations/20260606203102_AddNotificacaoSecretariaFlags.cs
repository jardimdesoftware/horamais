using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificacaoSecretariaFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificadoSecretariaComplementar",
                table: "Alunos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotificadoSecretariaExtensao",
                table: "Alunos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificadoSecretariaComplementar",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "NotificadoSecretariaExtensao",
                table: "Alunos");
        }
    }
}
