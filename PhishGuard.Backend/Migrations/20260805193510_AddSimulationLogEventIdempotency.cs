using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSimulationLogEventIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normaliza o histórico antes da restrição: eventos repetidos do mesmo
            // funil são semanticamente idênticos. Preserva sempre o registro mais
            // antigo (data_hora + id) e elimina apenas as repetições exatas da chave.
            migrationBuilder.Sql(
                """
                DELETE FROM simulations_logs AS duplicate
                USING simulations_logs AS original
                WHERE duplicate.campaign_id = original.campaign_id
                  AND duplicate.target_id = original.target_id
                  AND duplicate.acao = original.acao
                  AND (duplicate.data_hora, duplicate.id) > (original.data_hora, original.id);
                """);

            migrationBuilder.DropIndex(
                name: "ix_simulations_logs_campaign_id",
                table: "simulations_logs");

            migrationBuilder.CreateIndex(
                name: "ux_simulations_logs_campaign_target_action",
                table: "simulations_logs",
                columns: new[] { "campaign_id", "target_id", "acao" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_simulations_logs_campaign_target_action",
                table: "simulations_logs");

            migrationBuilder.CreateIndex(
                name: "ix_simulations_logs_campaign_id",
                table: "simulations_logs",
                column: "campaign_id");
        }
    }
}
