using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AjusteNomeTabelaSmtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmtpConfigs_Tenants_TenantId",
                table: "SmtpConfigs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SmtpConfigs",
                table: "SmtpConfigs");

            migrationBuilder.RenameTable(
                name: "SmtpConfigs",
                newName: "smtp_configs");

            migrationBuilder.RenameIndex(
                name: "IX_SmtpConfigs_TenantId",
                table: "smtp_configs",
                newName: "IX_smtp_configs_TenantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_smtp_configs",
                table: "smtp_configs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_smtp_configs_Tenants_TenantId",
                table: "smtp_configs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_smtp_configs_Tenants_TenantId",
                table: "smtp_configs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_smtp_configs",
                table: "smtp_configs");

            migrationBuilder.RenameTable(
                name: "smtp_configs",
                newName: "SmtpConfigs");

            migrationBuilder.RenameIndex(
                name: "IX_smtp_configs_TenantId",
                table: "SmtpConfigs",
                newName: "IX_SmtpConfigs_TenantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SmtpConfigs",
                table: "SmtpConfigs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SmtpConfigs_Tenants_TenantId",
                table: "SmtpConfigs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
