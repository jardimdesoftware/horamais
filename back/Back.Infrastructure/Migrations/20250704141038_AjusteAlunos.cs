using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjusteAlunos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlunoId",
                table: "Certificados");

            migrationBuilder.RenameColumn(
                name: "AtividadeId",
                table: "Certificados",
                newName: "AlunoAtividadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_AlunoAtividadeId",
                table: "Certificados",
                column: "AlunoAtividadeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificados_AlunoAtividades_AlunoAtividadeId",
                table: "Certificados",
                column: "AlunoAtividadeId",
                principalTable: "AlunoAtividades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificados_AlunoAtividades_AlunoAtividadeId",
                table: "Certificados");

            migrationBuilder.DropIndex(
                name: "IX_Certificados_AlunoAtividadeId",
                table: "Certificados");

            migrationBuilder.RenameColumn(
                name: "AlunoAtividadeId",
                table: "Certificados",
                newName: "AtividadeId");

            migrationBuilder.AddColumn<Guid>(
                name: "AlunoId",
                table: "Certificados",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
