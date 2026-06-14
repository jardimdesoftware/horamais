using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCursoIdFromAtividade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Atividades_Cursos_CursoId",
                table: "Atividades");

            migrationBuilder.DropIndex(
                name: "IX_Atividades_CursoId",
                table: "Atividades");

            migrationBuilder.DropColumn(
                name: "CursoId",
                table: "Atividades");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CursoId",
                table: "Atividades",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Atividades_CursoId",
                table: "Atividades",
                column: "CursoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Atividades_Cursos_CursoId",
                table: "Atividades",
                column: "CursoId",
                principalTable: "Cursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
