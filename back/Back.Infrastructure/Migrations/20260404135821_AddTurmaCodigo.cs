using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTurmaCodigo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Turmas",
                type: "character varying(6)",
                maxLength: 6,
                nullable: true);

            // Populate existing turmas with a random-ish code
            // Note: substring(md5(random()::text), 1, 6) is a simple way in Postgres to get some random characters
            migrationBuilder.Sql("UPDATE \"Turmas\" SET \"Codigo\" = UPPER(SUBSTRING(MD5(RANDOM()::TEXT), 1, 6)) WHERE \"Codigo\" IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Turmas",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turmas_Codigo",
                table: "Turmas",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Turmas_Codigo",
                table: "Turmas");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Turmas");
        }
    }
}
