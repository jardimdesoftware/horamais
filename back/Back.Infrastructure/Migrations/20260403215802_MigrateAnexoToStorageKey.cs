using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrateAnexoToStorageKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anexo",
                table: "Certificados");

            migrationBuilder.DropColumn(
                name: "AnexoContentType",
                table: "Certificados");

            migrationBuilder.AddColumn<string>(
                name: "AnexoStorageKey",
                table: "Certificados",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnexoStorageKey",
                table: "Certificados");

            migrationBuilder.AddColumn<byte[]>(
                name: "Anexo",
                table: "Certificados",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "AnexoContentType",
                table: "Certificados",
                type: "text",
                nullable: true);
        }
    }
}
