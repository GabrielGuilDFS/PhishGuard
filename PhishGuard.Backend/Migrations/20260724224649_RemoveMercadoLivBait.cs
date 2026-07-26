using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <summary>
    /// Remoção DEFINITIVA do cenário "Mercado Liv" (isca, landing e treinamento
    /// Just-in-Time), retirado do catálogo/código. Apaga as linhas de Template/
    /// PhishingPage/EducationalPage do cenário em TODOS os tenants, casando pela
    /// chave estável (`corpo_html` / `html_captura` / marcador em `conteudo_html`).
    ///
    /// GUARDA de integridade: campaigns→templates/phishing_pages/educational_pages
    /// é `DeleteBehavior.Restrict` (FK), então uma linha ainda referenciada por uma
    /// campanha existente NÃO pode ser apagada (o DELETE violaria a FK). Por isso
    /// cada DELETE é condicionado a "NOT IN (SELECT ... FROM campaigns)" — apaga o
    /// que está órfão e, para o que sobrar (campanha legada ainda referenciando o
    /// cenário retirado), renomeia para um rótulo genérico sem a marca, garantindo
    /// que nenhuma tela (ex.: tabela de Campanhas) volte a exibir "Mercado Liv".
    /// </summary>
    public partial class RemoveMercadoLivBait : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Templates (isca de e-mail) órfãos: apaga. Referenciados: renomeia.
            migrationBuilder.Sql(@"
                DELETE FROM templates
                WHERE corpo_html = 'mercado-liv-novo-acesso'
                  AND id NOT IN (SELECT email_template_id FROM campaigns);
            ");
            migrationBuilder.Sql(@"
                UPDATE templates
                SET nome = 'Cenário descontinuado', remetente_nome = 'Simulação PhishGuard'
                WHERE corpo_html = 'mercado-liv-novo-acesso';
            ");

            // Páginas falsas (landing) órfãs: apaga. Referenciadas: renomeia.
            migrationBuilder.Sql(@"
                DELETE FROM phishing_pages
                WHERE html_captura = 'mercado-liv-login'
                  AND id NOT IN (SELECT landing_page_id FROM campaigns);
            ");
            migrationBuilder.Sql(@"
                UPDATE phishing_pages
                SET nome = 'Cenário descontinuado'
                WHERE html_captura = 'mercado-liv-login';
            ");

            // Página educativa (treinamento JIT) órfã: apaga. Referenciada: renomeia.
            // O conteúdo é o marcador estável gerado por resolverPaginaEducativaDoCenario
            // (data-feedback-training="mercadoliv"), não um slug de arquivo.
            migrationBuilder.Sql(@"
                DELETE FROM educational_pages
                WHERE html_educacional LIKE '%data-feedback-training=""mercadoliv""%'
                  AND id NOT IN (SELECT educational_page_id FROM campaigns);
            ");
            migrationBuilder.Sql(@"
                UPDATE educational_pages
                SET nome = 'Cenário descontinuado'
                WHERE html_educacional LIKE '%data-feedback-training=""mercadoliv""%';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sem reversão: o cenário foi retirado do código-fonte (catálogo, isca
            // embutida e landing não existem mais) — não há HTML para restaurar.
        }
    }
}
