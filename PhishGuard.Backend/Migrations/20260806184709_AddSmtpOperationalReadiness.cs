using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSmtpOperationalReadiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "senha",
                table: "smtp_configs",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "ultimo_erro_codigo",
                table: "smtp_configs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ultimo_teste_em_utc",
                table: "smtp_configs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ultimo_teste_sucesso",
                table: "smtp_configs",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dispatch_attempt_count",
                table: "campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "dispatch_error_code",
                table: "campaigns",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dispatch_error_message",
                table: "campaigns",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "dispatch_failed_at_utc",
                table: "campaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(type: "text", nullable: true),
                    xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropColumn(
                name: "ultimo_erro_codigo",
                table: "smtp_configs");

            migrationBuilder.DropColumn(
                name: "ultimo_teste_em_utc",
                table: "smtp_configs");

            migrationBuilder.DropColumn(
                name: "ultimo_teste_sucesso",
                table: "smtp_configs");

            migrationBuilder.DropColumn(
                name: "dispatch_attempt_count",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "dispatch_error_code",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "dispatch_error_message",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "dispatch_failed_at_utc",
                table: "campaigns");

            migrationBuilder.AlterColumn<string>(
                name: "senha",
                table: "smtp_configs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048);
        }
    }
}
