using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminLockoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "acesso_falhas_contador",
                table: "administradores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "bloqueio_fim",
                table: "administradores",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "acesso_falhas_contador",
                table: "administradores");

            migrationBuilder.DropColumn(
                name: "bloqueio_fim",
                table: "administradores");
        }
    }
}
