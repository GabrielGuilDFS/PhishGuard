using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <summary>
    /// Restaura os rótulos canônicos do cenário Mercado Liv reativado.
    ///
    /// O reparo é deliberadamente restrito ao antigo rótulo de descontinuação e aos
    /// identificadores/marcador estáveis do próprio cenário. Assim, preserva IDs e FKs
    /// de campanhas históricas sem renomear outros cenários descontinuados. Abrange
    /// todos os tenants porque migrations não possuem contexto HTTP, mas não mistura
    /// nem reassocia dados entre eles.
    /// </summary>
    public partial class RestoreMercadoLivScenarioLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE templates
                SET nome = 'Mercado Liv - Novo Acesso Detectado',
                    remetente_nome = 'Mercado Liv'
                WHERE nome = 'Cenário descontinuado'
                  AND corpo_html IN ('mercado-liv-novo-acesso', 'mercadoliv-novo-acesso');
            ");

            migrationBuilder.Sql(@"
                UPDATE phishing_pages
                SET nome = 'Mercado Liv - Acesse sua conta'
                WHERE nome = 'Cenário descontinuado'
                  AND html_captura IN ('mercado-liv-login', 'mercadoliv-login');
            ");

            migrationBuilder.Sql(@"
                UPDATE educational_pages
                SET nome = 'Treinamento Interativo — Mercado Liv'
                WHERE nome = 'Cenário descontinuado'
                  AND html_educacional LIKE '%data-feedback-training=""mercadoliv""%';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sem reversão intencional: o nome anterior representava um estado legado
            // incorreto. Restaurá-lo faria campanhas válidas voltarem a aparecer como
            // descontinuadas.
        }
    }
}
