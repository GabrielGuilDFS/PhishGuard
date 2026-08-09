using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiProviderEmailDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "api_account_identifier",
                table: "smtp_configs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "api_provider",
                table: "smtp_configs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "api_region",
                table: "smtp_configs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "us-east-1");

            migrationBuilder.AddColumn<string>(
                name: "encrypted_api_key",
                table: "smtp_configs",
                type: "character varying(4096)",
                maxLength: 4096,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "provider_type",
                table: "smtp_configs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "sender_email",
                table: "smtp_configs",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "sender_name",
                table: "smtp_configs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "campaign_deliveries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    concurrency_token = table.Column<Guid>(type: "uuid", nullable: false),
                    last_attempt_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lease_expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    provider_message_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    last_error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaign_deliveries", x => x.id);
                    table.ForeignKey(
                        name: "fk_campaign_deliveries_alvos_target_id",
                        column: x => x.target_id,
                        principalTable: "alvos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_campaign_deliveries_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_campaign_deliveries_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_campaign_deliveries_target_id",
                table: "campaign_deliveries",
                column: "target_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaign_deliveries_tenant_status_lease",
                table: "campaign_deliveries",
                columns: new[] { "tenant_id", "status", "lease_expires_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_campaign_deliveries_campaign_target",
                table: "campaign_deliveries",
                columns: new[] { "campaign_id", "target_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaign_deliveries");

            migrationBuilder.DropColumn(
                name: "api_account_identifier",
                table: "smtp_configs");

            migrationBuilder.DropColumn(
                name: "api_provider",
                table: "smtp_configs");

            migrationBuilder.DropColumn(
                name: "api_region",
                table: "smtp_configs");

            migrationBuilder.DropColumn(
                name: "encrypted_api_key",
                table: "smtp_configs");

            migrationBuilder.DropColumn(
                name: "provider_type",
                table: "smtp_configs");

            migrationBuilder.DropColumn(
                name: "sender_email",
                table: "smtp_configs");

            migrationBuilder.DropColumn(
                name: "sender_name",
                table: "smtp_configs");
        }
    }
}
