using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeCursosCampusIdRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Campi_CampusId",
                table: "Cursos");

            migrationBuilder.Sql(@"
                INSERT INTO ""Campi"" (""Id"", ""Nome"", ""Cidade"")
                SELECT (md5(random()::text || clock_timestamp()::text)::uuid), 'Campus Padrão', 'Não informado'
                WHERE NOT EXISTS (SELECT 1 FROM ""Campi"");
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Cursos""
                SET ""CampusId"" = (SELECT ""Id"" FROM ""Campi"" ORDER BY ""Nome"" LIMIT 1)
                WHERE ""CampusId"" IS NULL;
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "CampusId",
                table: "Cursos",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Campi_CampusId",
                table: "Cursos",
                column: "CampusId",
                principalTable: "Campi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Campi_CampusId",
                table: "Cursos");

            migrationBuilder.AlterColumn<Guid>(
                name: "CampusId",
                table: "Cursos",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Campi_CampusId",
                table: "Cursos",
                column: "CampusId",
                principalTable: "Campi",
                principalColumn: "Id");
        }
    }
}
