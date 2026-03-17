using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Cnpj",
                table: "Tenants",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Administradores_Email",
                table: "Administradores",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_Cnpj",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Administradores_Email",
                table: "Administradores");
        }
    }
}
