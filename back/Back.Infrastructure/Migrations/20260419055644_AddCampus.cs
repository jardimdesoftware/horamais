using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCampus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaximoHorasExtensao",
                table: "LimitesHoras");

            migrationBuilder.AddColumn<int>(
                name: "MaximoHorasExtensao",
                table: "Turmas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CampusId",
                table: "Cursos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Campi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Cidade = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campi", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_CampusId",
                table: "Cursos",
                column: "CampusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Campi_CampusId",
                table: "Cursos",
                column: "CampusId",
                principalTable: "Campi",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Campi_CampusId",
                table: "Cursos");

            migrationBuilder.DropTable(
                name: "Campi");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_CampusId",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "MaximoHorasExtensao",
                table: "Turmas");

            migrationBuilder.DropColumn(
                name: "CampusId",
                table: "Cursos");

            migrationBuilder.AddColumn<int>(
                name: "MaximoHorasExtensao",
                table: "LimitesHoras",
                type: "integer",
                nullable: true);
        }
    }
}
