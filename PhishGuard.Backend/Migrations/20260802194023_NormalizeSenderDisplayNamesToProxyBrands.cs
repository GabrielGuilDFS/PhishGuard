using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <summary>
    /// Data-fix DEFENSIVO do NOME DE EXIBIÇÃO do remetente (vira o "Friendly Name" do
    /// header From no Gmail/Outlook) e dos rótulos exibidos (Template.Nome / Assunto /
    /// PhishingPage.Nome).
    ///
    /// A migração anterior <see cref="SanitizeLegacyBaitNamesParody"/> higieniza casando
    /// pelo SLUG estável em corpo_html/html_captura. Isso deixa uma brecha: como cada
    /// campanha faz find-or-create do template pelo slug (Campaigns.tsx →
    /// garantirCenario), uma linha criada com um build ANTIGO do frontend — quando
    /// remetente_nome ainda trazia a marca REAL ("Netflix") — é REUTILIZADA para sempre e
    /// nunca mais é reescrita; a migração keyada por slug já rodou e não volta. Resultado:
    /// o alvo recebe o e-mail com "Netflix" no From, mesmo com todo o código já em
    /// "NetsFlix". Este fix ataca o problema pela RAIZ: normaliza pelo próprio VALOR da
    /// marca, alcançando qualquer linha independentemente do corpo_html.
    ///
    /// Idempotente (após a troca o token real some) e cobre TODOS os tenants. Sem reversão:
    /// restaurar a marca real é justamente o que queremos evitar (compliance de IP).
    /// </summary>
    public partial class NormalizeSenderDisplayNamesToProxyBrands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) NOME DE EXIBIÇÃO DO REMETENTE — normaliza por VALOR (case-insensitive),
            //    cobrindo qualquer linha legada, não só as que casam pelo slug.
            migrationBuilder.Sql("UPDATE templates SET remetente_nome = 'NetsFlix'     WHERE lower(remetente_nome) IN ('netflix');");
            migrationBuilder.Sql("UPDATE templates SET remetente_nome = 'bho MAX'      WHERE lower(remetente_nome) IN ('hbo max', 'hbo', 'hbomax', 'hbo max brasil');");
            migrationBuilder.Sql("UPDATE templates SET remetente_nome = 'amzprime'     WHERE lower(remetente_nome) IN ('amazon', 'amazon prime', 'amazon.com.br', 'prime video');");
            migrationBuilder.Sql("UPDATE templates SET remetente_nome = 'Microsft 365' WHERE lower(remetente_nome) IN ('microsoft 365', 'microsoft', 'office 365', 'microsoft office 365');");
            migrationBuilder.Sql("UPDATE templates SET remetente_nome = 'Mercado Liv'  WHERE lower(remetente_nome) IN ('mercado livre', 'mercadolivre', 'mercado pago', 'mercadolibre');");

            // 2) RÓTULOS EXIBIDOS (nome/assunto do template e nome da página) — troca o token
            //    da marca real por substring. Idempotente: depois da troca o token some.
            foreach (var (real, proxy) in new[]
            {
                ("Netflix", "NetsFlix"),
                ("HBO Max", "bho MAX"),
                ("Amazon", "amzprime"),
                ("Microsoft 365", "Microsft 365"),
                ("Microsoft", "Microsft"),
                ("Mercado Livre", "Mercado Liv"),
            })
            {
                migrationBuilder.Sql($"UPDATE templates SET nome = replace(nome, '{real}', '{proxy}') WHERE nome LIKE '%{real}%';");
                migrationBuilder.Sql($"UPDATE templates SET assunto = replace(assunto, '{real}', '{proxy}') WHERE assunto LIKE '%{real}%';");
                migrationBuilder.Sql($"UPDATE phishing_pages SET nome = replace(nome, '{real}', '{proxy}') WHERE nome LIKE '%{real}%';");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sem reversão intencional: reintroduzir a marca REAL viola o compliance de IP.
        }
    }
}
