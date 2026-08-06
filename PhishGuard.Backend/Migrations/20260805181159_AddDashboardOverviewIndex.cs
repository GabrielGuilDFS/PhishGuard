using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardOverviewIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_simulations_logs_tenant_id",
                table: "simulations_logs");

            migrationBuilder.CreateIndex(
                name: "ix_simulations_logs_dashboard_overview",
                table: "simulations_logs",
                columns: new[] { "tenant_id", "data_hora", "acao", "campaign_id", "target_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_simulations_logs_dashboard_overview",
                table: "simulations_logs");

            migrationBuilder.CreateIndex(
                name: "ix_simulations_logs_tenant_id",
                table: "simulations_logs",
                column: "tenant_id");
        }
    }
}
