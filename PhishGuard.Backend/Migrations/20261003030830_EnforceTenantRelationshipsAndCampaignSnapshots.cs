using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <inheritdoc />
    public partial class EnforceTenantRelationshipsAndCampaignSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fail before changing schema if existing relationships cannot be preserved.
            ValidateLegacyRelationships(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "fk_auth_sessions_administradores_administrator_id",
                table: "auth_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_campaign_deliveries_alvos_target_id",
                table: "campaign_deliveries");

            migrationBuilder.DropForeignKey(
                name: "fk_campaign_deliveries_campaigns_campaign_id",
                table: "campaign_deliveries");

            migrationBuilder.DropForeignKey(
                name: "fk_campaign_target_alvos_targets_id",
                table: "campaign_target");

            migrationBuilder.DropForeignKey(
                name: "fk_campaign_target_campaigns_campaigns_id",
                table: "campaign_target");

            migrationBuilder.DropForeignKey(
                name: "fk_campaigns_educational_pages_educational_page_id",
                table: "campaigns");

            migrationBuilder.DropForeignKey(
                name: "fk_campaigns_phishing_pages_landing_page_id",
                table: "campaigns");

            migrationBuilder.DropForeignKey(
                name: "fk_campaigns_templates_email_template_id",
                table: "campaigns");

            migrationBuilder.DropForeignKey(
                name: "fk_simulations_logs_alvos_target_id",
                table: "simulations_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_simulations_logs_campaigns_campaign_id",
                table: "simulations_logs");

            migrationBuilder.DropIndex(
                name: "ix_templates_tenant_id",
                table: "templates");

            migrationBuilder.DropIndex(
                name: "ix_simulations_logs_target_id",
                table: "simulations_logs");

            migrationBuilder.DropIndex(
                name: "ix_phishing_pages_tenant_id",
                table: "phishing_pages");

            migrationBuilder.DropIndex(
                name: "ix_educational_pages_tenant_id",
                table: "educational_pages");

            migrationBuilder.DropIndex(
                name: "ix_campaigns_educational_page_id",
                table: "campaigns");

            migrationBuilder.DropIndex(
                name: "ix_campaigns_email_template_id",
                table: "campaigns");

            migrationBuilder.DropIndex(
                name: "ix_campaigns_landing_page_id",
                table: "campaigns");

            migrationBuilder.DropIndex(
                name: "ix_campaigns_tenant_id",
                table: "campaigns");

            migrationBuilder.DropIndex(
                name: "ix_campaign_target_targets_id",
                table: "campaign_target");

            migrationBuilder.DropIndex(
                name: "ix_campaign_deliveries_target_id",
                table: "campaign_deliveries");

            migrationBuilder.DropIndex(
                name: "ix_auth_sessions_administrator_id",
                table: "auth_sessions");

            migrationBuilder.DropIndex(
                name: "ix_administradores_tenant_id",
                table: "administradores");

            migrationBuilder.AddColumn<string>(
                name: "educational_feedback_config_json_snapshot",
                table: "campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "educational_html_snapshot",
                table: "campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "educational_name_snapshot",
                table: "campaigns",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_body_snapshot",
                table: "campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_subject_snapshot",
                table: "campaigns",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_legacy_snapshot",
                table: "campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "landing_html_snapshot",
                table: "campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "landing_name_snapshot",
                table: "campaigns",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "landing_template_id_snapshot",
                table: "campaigns",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sender_email_snapshot",
                table: "campaigns",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sender_name_snapshot",
                table: "campaigns",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "snapshot_captured_at_utc",
                table: "campaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "template_name_snapshot",
                table: "campaigns",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "department_snapshot",
                table: "campaign_target",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_legacy_snapshot",
                table: "campaign_target",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "snapshot_captured_at_utc",
                table: "campaign_target",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "target_email_snapshot",
                table: "campaign_target",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "target_name_snapshot",
                table: "campaign_target",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "campaign_target",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            BackfillLegacySnapshots(migrationBuilder);
            MaterializeLegacyOfficialContent(migrationBuilder);

            migrationBuilder.AddUniqueConstraint(
                name: "ak_templates_tenant_id_id",
                table: "templates",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_phishing_pages_tenant_id_id",
                table: "phishing_pages",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_educational_pages_tenant_id_id",
                table: "educational_pages",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_campaigns_tenant_id_id",
                table: "campaigns",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_campaign_target_tenant_id_campaigns_id_targets_id",
                table: "campaign_target",
                columns: new[] { "tenant_id", "campaigns_id", "targets_id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_alvos_tenant_id_id",
                table: "alvos",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_administradores_tenant_id_id",
                table: "administradores",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_simulations_logs_tenant_id_campaign_id_target_id",
                table: "simulations_logs",
                columns: new[] { "tenant_id", "campaign_id", "target_id" });

            migrationBuilder.CreateIndex(
                name: "ix_simulations_logs_tenant_id_target_id",
                table: "simulations_logs",
                columns: new[] { "tenant_id", "target_id" });

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_tenant_id_educational_page_id",
                table: "campaigns",
                columns: new[] { "tenant_id", "educational_page_id" });

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_tenant_id_email_template_id",
                table: "campaigns",
                columns: new[] { "tenant_id", "email_template_id" });

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_tenant_id_landing_page_id",
                table: "campaigns",
                columns: new[] { "tenant_id", "landing_page_id" });

            migrationBuilder.CreateIndex(
                name: "ix_campaign_target_tenant_id_targets_id",
                table: "campaign_target",
                columns: new[] { "tenant_id", "targets_id" });

            migrationBuilder.CreateIndex(
                name: "ix_campaign_deliveries_tenant_id_campaign_id_target_id",
                table: "campaign_deliveries",
                columns: new[] { "tenant_id", "campaign_id", "target_id" });

            migrationBuilder.CreateIndex(
                name: "ix_campaign_deliveries_tenant_id_target_id",
                table: "campaign_deliveries",
                columns: new[] { "tenant_id", "target_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_auth_sessions_administradores_tenant_id_administrator_id",
                table: "auth_sessions",
                columns: new[] { "tenant_id", "administrator_id" },
                principalTable: "administradores",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_deliveries_alvos_tenant_id_target_id",
                table: "campaign_deliveries",
                columns: new[] { "tenant_id", "target_id" },
                principalTable: "alvos",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_deliveries_participant",
                table: "campaign_deliveries",
                columns: new[] { "tenant_id", "campaign_id", "target_id" },
                principalTable: "campaign_target",
                principalColumns: new[] { "tenant_id", "campaigns_id", "targets_id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_deliveries_campaigns_tenant_id_campaign_id",
                table: "campaign_deliveries",
                columns: new[] { "tenant_id", "campaign_id" },
                principalTable: "campaigns",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_target_alvos_tenant_id_targets_id",
                table: "campaign_target",
                columns: new[] { "tenant_id", "targets_id" },
                principalTable: "alvos",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_target_campaigns_tenant_id_campaigns_id",
                table: "campaign_target",
                columns: new[] { "tenant_id", "campaigns_id" },
                principalTable: "campaigns",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_campaigns_educational_pages_tenant_id_educational_page_id",
                table: "campaigns",
                columns: new[] { "tenant_id", "educational_page_id" },
                principalTable: "educational_pages",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_campaigns_phishing_pages_tenant_id_landing_page_id",
                table: "campaigns",
                columns: new[] { "tenant_id", "landing_page_id" },
                principalTable: "phishing_pages",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_campaigns_templates_tenant_id_email_template_id",
                table: "campaigns",
                columns: new[] { "tenant_id", "email_template_id" },
                principalTable: "templates",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_simulations_logs_alvos_tenant_id_target_id",
                table: "simulations_logs",
                columns: new[] { "tenant_id", "target_id" },
                principalTable: "alvos",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_simulations_logs_participant",
                table: "simulations_logs",
                columns: new[] { "tenant_id", "campaign_id", "target_id" },
                principalTable: "campaign_target",
                principalColumns: new[] { "tenant_id", "campaigns_id", "targets_id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_simulations_logs_campaigns_tenant_id_campaign_id",
                table: "simulations_logs",
                columns: new[] { "tenant_id", "campaign_id" },
                principalTable: "campaigns",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_auth_sessions_administradores_tenant_id_administrator_id",
                table: "auth_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_campaign_deliveries_alvos_tenant_id_target_id",
                table: "campaign_deliveries");

            migrationBuilder.DropForeignKey(
                name: "fk_campaign_deliveries_participant",
                table: "campaign_deliveries");

            migrationBuilder.DropForeignKey(
                name: "fk_campaign_deliveries_campaigns_tenant_id_campaign_id",
                table: "campaign_deliveries");

            migrationBuilder.DropForeignKey(
                name: "fk_campaign_target_alvos_tenant_id_targets_id",
                table: "campaign_target");

            migrationBuilder.DropForeignKey(
                name: "fk_campaign_target_campaigns_tenant_id_campaigns_id",
                table: "campaign_target");

            migrationBuilder.DropForeignKey(
                name: "fk_campaigns_educational_pages_tenant_id_educational_page_id",
                table: "campaigns");

            migrationBuilder.DropForeignKey(
                name: "fk_campaigns_phishing_pages_tenant_id_landing_page_id",
                table: "campaigns");

            migrationBuilder.DropForeignKey(
                name: "fk_campaigns_templates_tenant_id_email_template_id",
                table: "campaigns");

            migrationBuilder.DropForeignKey(
                name: "fk_simulations_logs_alvos_tenant_id_target_id",
                table: "simulations_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_simulations_logs_participant",
                table: "simulations_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_simulations_logs_campaigns_tenant_id_campaign_id",
                table: "simulations_logs");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_templates_tenant_id_id",
                table: "templates");

            migrationBuilder.DropIndex(
                name: "ix_simulations_logs_tenant_id_campaign_id_target_id",
                table: "simulations_logs");

            migrationBuilder.DropIndex(
                name: "ix_simulations_logs_tenant_id_target_id",
                table: "simulations_logs");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_phishing_pages_tenant_id_id",
                table: "phishing_pages");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_educational_pages_tenant_id_id",
                table: "educational_pages");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_campaigns_tenant_id_id",
                table: "campaigns");

            migrationBuilder.DropIndex(
                name: "ix_campaigns_tenant_id_educational_page_id",
                table: "campaigns");

            migrationBuilder.DropIndex(
                name: "ix_campaigns_tenant_id_email_template_id",
                table: "campaigns");

            migrationBuilder.DropIndex(
                name: "ix_campaigns_tenant_id_landing_page_id",
                table: "campaigns");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_campaign_target_tenant_id_campaigns_id_targets_id",
                table: "campaign_target");

            migrationBuilder.DropIndex(
                name: "ix_campaign_target_tenant_id_targets_id",
                table: "campaign_target");

            migrationBuilder.DropIndex(
                name: "ix_campaign_deliveries_tenant_id_campaign_id_target_id",
                table: "campaign_deliveries");

            migrationBuilder.DropIndex(
                name: "ix_campaign_deliveries_tenant_id_target_id",
                table: "campaign_deliveries");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_alvos_tenant_id_id",
                table: "alvos");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_administradores_tenant_id_id",
                table: "administradores");

            migrationBuilder.DropColumn(
                name: "educational_feedback_config_json_snapshot",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "educational_html_snapshot",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "educational_name_snapshot",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "email_body_snapshot",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "email_subject_snapshot",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "is_legacy_snapshot",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "landing_html_snapshot",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "landing_name_snapshot",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "landing_template_id_snapshot",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "sender_email_snapshot",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "sender_name_snapshot",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "snapshot_captured_at_utc",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "template_name_snapshot",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "department_snapshot",
                table: "campaign_target");

            migrationBuilder.DropColumn(
                name: "is_legacy_snapshot",
                table: "campaign_target");

            migrationBuilder.DropColumn(
                name: "snapshot_captured_at_utc",
                table: "campaign_target");

            migrationBuilder.DropColumn(
                name: "target_email_snapshot",
                table: "campaign_target");

            migrationBuilder.DropColumn(
                name: "target_name_snapshot",
                table: "campaign_target");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "campaign_target");

            migrationBuilder.CreateIndex(
                name: "ix_templates_tenant_id",
                table: "templates",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_simulations_logs_target_id",
                table: "simulations_logs",
                column: "target_id");

            migrationBuilder.CreateIndex(
                name: "ix_phishing_pages_tenant_id",
                table: "phishing_pages",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_educational_pages_tenant_id",
                table: "educational_pages",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_educational_page_id",
                table: "campaigns",
                column: "educational_page_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_email_template_id",
                table: "campaigns",
                column: "email_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_landing_page_id",
                table: "campaigns",
                column: "landing_page_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_tenant_id",
                table: "campaigns",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaign_target_targets_id",
                table: "campaign_target",
                column: "targets_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaign_deliveries_target_id",
                table: "campaign_deliveries",
                column: "target_id");

            migrationBuilder.CreateIndex(
                name: "ix_auth_sessions_administrator_id",
                table: "auth_sessions",
                column: "administrator_id");

            migrationBuilder.CreateIndex(
                name: "ix_administradores_tenant_id",
                table: "administradores",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_auth_sessions_administradores_administrator_id",
                table: "auth_sessions",
                column: "administrator_id",
                principalTable: "administradores",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_deliveries_alvos_target_id",
                table: "campaign_deliveries",
                column: "target_id",
                principalTable: "alvos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_deliveries_campaigns_campaign_id",
                table: "campaign_deliveries",
                column: "campaign_id",
                principalTable: "campaigns",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_target_alvos_targets_id",
                table: "campaign_target",
                column: "targets_id",
                principalTable: "alvos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_campaign_target_campaigns_campaigns_id",
                table: "campaign_target",
                column: "campaigns_id",
                principalTable: "campaigns",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_campaigns_educational_pages_educational_page_id",
                table: "campaigns",
                column: "educational_page_id",
                principalTable: "educational_pages",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_campaigns_phishing_pages_landing_page_id",
                table: "campaigns",
                column: "landing_page_id",
                principalTable: "phishing_pages",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_campaigns_templates_email_template_id",
                table: "campaigns",
                column: "email_template_id",
                principalTable: "templates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_simulations_logs_alvos_target_id",
                table: "simulations_logs",
                column: "target_id",
                principalTable: "alvos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_simulations_logs_campaigns_campaign_id",
                table: "simulations_logs",
                column: "campaign_id",
                principalTable: "campaigns",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
