using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignConcurrencyToken : Migration
    {
        // Mapeia o token de concorrência otimista de Campaign para a coluna de SISTEMA
        // 'xmin' do PostgreSQL (UseXminAsConcurrencyToken no AppDbContext).
        //
        // 'xmin' JÁ EXISTE fisicamente em toda tabela do Postgres — não é coluna de
        // usuário. Logo NÃO há DDL a executar: o AddColumn/DropColumn que o diff do EF
        // gerou por padrão foi removido de propósito (aplicá-lo lançaria "column name
        // 'xmin' conflicts with a system column name"). O valor desta migration é
        // sincronizar o ModelSnapshot com o mapeamento de concorrência.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: 'xmin' é coluna de sistema (ver nota acima).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op.
        }
    }
}
