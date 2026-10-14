using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ProtectMvpCampaignHistoryAndTargetUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_campaign_deliveries_alvos_target_id",
                table: "campaign_deliveries");

            migrationBuilder.DropForeignKey(
                name: "fk_campaign_target_alvos_targets_id",
                table: "campaign_target");

            migrationBuilder.DropForeignKey(
                name: "fk_simulations_logs_alvos_target_id",
                table: "simulations_logs");

            migrationBuilder.DropIndex(
                name: "ix_alvos_tenant_id",
                table: "alvos");

            migrationBuilder.AddColumn<string>(
                name: "email_normalizado",
                table: "alvos",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            // Nunca resolve duplicidades apagando destinatários ou histórico.
            // O teste e o backfill executam na transação da migration.
            migrationBuilder.Sql("""
                -- Trim usa os mesmos espaços Unicode de String.Trim(). A conversão
                -- por caractere evita regras contextuais; U+0130 é preservado pelo .NET.
                UPDATE alvos SET email_normalizado = (
                    SELECT string_agg(CASE
                        WHEN ascii(part) BETWEEN 65 AND 90 THEN chr(ascii(part) + 32)
                        WHEN ascii(part) = 304 THEN part
                        ELSE lower(part) END, '' ORDER BY ordinal)
                    FROM regexp_split_to_table(
                        btrim(email, U&'\0009\000A\000B\000C\000D\0020\0085\00A0\1680\2000\2001\2002\2003\2004\2005\2006\2007\2008\2009\200A\2028\2029\202F\205F\3000'),
                        '') WITH ORDINALITY AS letters(part, ordinal)
                );
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM alvos GROUP BY tenant_id, email_normalizado HAVING count(*) > 1) THEN
                        RAISE EXCEPTION 'Existem emails duplicados por tenant. Revise os destinatarios antes de aplicar esta migration; nenhum historico foi removido.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "email_normalizado", table: "alvos", type: "character varying(150)",
                maxLength: 150, nullable: false, oldClrType: typeof(string),
                oldType: "character varying(150)", oldMaxLength: 150, oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_alvos_tenant_email_normalizado",
                table: "alvos",
                columns: new[] { "tenant_id", "email_normalizado" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_deliveries_alvos_target_id",
                table: "campaign_deliveries",
                column: "target_id",
                principalTable: "alvos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_target_alvos_targets_id",
                table: "campaign_target",
                column: "targets_id",
                principalTable: "alvos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_simulations_logs_alvos_target_id",
                table: "simulations_logs",
                column: "target_id",
                principalTable: "alvos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_campaign_deliveries_alvos_target_id",
                table: "campaign_deliveries");

            migrationBuilder.DropForeignKey(
                name: "fk_campaign_target_alvos_targets_id",
                table: "campaign_target");

            migrationBuilder.DropForeignKey(
                name: "fk_simulations_logs_alvos_target_id",
                table: "simulations_logs");

            migrationBuilder.DropIndex(
                name: "ux_alvos_tenant_email_normalizado",
                table: "alvos");

            migrationBuilder.DropColumn(
                name: "email_normalizado",
                table: "alvos");

            migrationBuilder.CreateIndex(
                name: "ix_alvos_tenant_id",
                table: "alvos",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_deliveries_alvos_target_id",
                table: "campaign_deliveries",
                column: "target_id",
                principalTable: "alvos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_target_alvos_targets_id",
                table: "campaign_target",
                column: "targets_id",
                principalTable: "alvos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_simulations_logs_alvos_target_id",
                table: "simulations_logs",
                column: "target_id",
                principalTable: "alvos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
