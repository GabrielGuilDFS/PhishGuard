using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPhysicalForeignKeysAndRestrictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SANEAMENTO PRÉVIO (auto-contido): antes de criar as FKs de simulations_logs,
            // remove logs órfãos herdados (registros cujo campaign/target/tenant já não
            // existe — exatamente o problema que estas FKs passam a impedir). Sem isto, o
            // AddForeignKey falharia por violação de integridade nos dados existentes.
            // Nomes ainda em PascalCase aqui: o snake_case é aplicado na migration seguinte.
            migrationBuilder.Sql(
                "DELETE FROM simulations_logs l WHERE NOT EXISTS (SELECT 1 FROM \"Campaigns\" c WHERE c.\"Id\" = l.campaign_id);");
            migrationBuilder.Sql(
                "DELETE FROM simulations_logs l WHERE NOT EXISTS (SELECT 1 FROM \"Alvos\" a WHERE a.\"Id\" = l.target_id);");
            migrationBuilder.Sql(
                "DELETE FROM simulations_logs l WHERE NOT EXISTS (SELECT 1 FROM \"Tenants\" t WHERE t.\"Id\" = l.tenant_id);");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_EducationalPages_EducationalPageId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_PhishingPages_LandingPageId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Templates_EmailTemplateId",
                table: "Campaigns");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_TenantId",
                table: "Templates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_simulations_logs_campaign_id",
                table: "simulations_logs",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_simulations_logs_target_id",
                table: "simulations_logs",
                column: "target_id");

            migrationBuilder.CreateIndex(
                name: "IX_simulations_logs_tenant_id",
                table: "simulations_logs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_PhishingPages_TenantId",
                table: "PhishingPages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalPages_TenantId",
                table: "EducationalPages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_TenantId",
                table: "Campaigns",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_EducationalPages_EducationalPageId",
                table: "Campaigns",
                column: "EducationalPageId",
                principalTable: "EducationalPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_PhishingPages_LandingPageId",
                table: "Campaigns",
                column: "LandingPageId",
                principalTable: "PhishingPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Templates_EmailTemplateId",
                table: "Campaigns",
                column: "EmailTemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Tenants_TenantId",
                table: "Campaigns",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalPages_Tenants_TenantId",
                table: "EducationalPages",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhishingPages_Tenants_TenantId",
                table: "PhishingPages",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_simulations_logs_Alvos_target_id",
                table: "simulations_logs",
                column: "target_id",
                principalTable: "Alvos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_simulations_logs_Campaigns_campaign_id",
                table: "simulations_logs",
                column: "campaign_id",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_simulations_logs_Tenants_tenant_id",
                table: "simulations_logs",
                column: "tenant_id",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Templates_Tenants_TenantId",
                table: "Templates",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_EducationalPages_EducationalPageId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_PhishingPages_LandingPageId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Templates_EmailTemplateId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Tenants_TenantId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_EducationalPages_Tenants_TenantId",
                table: "EducationalPages");

            migrationBuilder.DropForeignKey(
                name: "FK_PhishingPages_Tenants_TenantId",
                table: "PhishingPages");

            migrationBuilder.DropForeignKey(
                name: "FK_simulations_logs_Alvos_target_id",
                table: "simulations_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_simulations_logs_Campaigns_campaign_id",
                table: "simulations_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_simulations_logs_Tenants_tenant_id",
                table: "simulations_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_Templates_Tenants_TenantId",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_TenantId",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_simulations_logs_campaign_id",
                table: "simulations_logs");

            migrationBuilder.DropIndex(
                name: "IX_simulations_logs_target_id",
                table: "simulations_logs");

            migrationBuilder.DropIndex(
                name: "IX_simulations_logs_tenant_id",
                table: "simulations_logs");

            migrationBuilder.DropIndex(
                name: "IX_PhishingPages_TenantId",
                table: "PhishingPages");

            migrationBuilder.DropIndex(
                name: "IX_EducationalPages_TenantId",
                table: "EducationalPages");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_TenantId",
                table: "Campaigns");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_EducationalPages_EducationalPageId",
                table: "Campaigns",
                column: "EducationalPageId",
                principalTable: "EducationalPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_PhishingPages_LandingPageId",
                table: "Campaigns",
                column: "LandingPageId",
                principalTable: "PhishingPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Templates_EmailTemplateId",
                table: "Campaigns",
                column: "EmailTemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
