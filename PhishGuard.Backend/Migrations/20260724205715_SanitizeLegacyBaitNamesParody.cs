using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <summary>
    /// Higieniza dados LEGADOS: linhas de Template/PhishingPage criadas antes do rebrand
    /// de paródia ainda carregam o NOME com a marca real (ex.: "Netflix - ...") na coluna
    /// exibida pela tela de campanhas (Template.Nome / PhishingPage.Nome). Este data-fix
    /// reescreve o Nome canônico parodiado do catálogo, casando pela chave ESTÁVEL: o slug
    /// da isca em `corpo_html` (templates) e o slug da landing em `html_captura`
    /// (phishing_pages). Só linhas de cenário oficial casam (o slug é texto curto); templates
    /// custom guardam HTML bruto e ficam intactos. Idempotente e cobre TODOS os tenants.
    /// </summary>
    public partial class SanitizeLegacyBaitNamesParody : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Iscas de e-mail (templates.corpo_html = id da isca).
            migrationBuilder.Sql("UPDATE templates SET nome = 'bho MAX - Redefinição de Senha' WHERE corpo_html = 'hbomax-redefinicao-senha';");
            migrationBuilder.Sql("UPDATE templates SET nome = 'NetsFlix - Atualização de Detalhes de Cobrança' WHERE corpo_html = 'netflix-atualizacao-cobranca';");
            migrationBuilder.Sql("UPDATE templates SET nome = 'amzprime - Notificação Geral' WHERE corpo_html = 'amazon-notificacao-geral';");
            migrationBuilder.Sql("UPDATE templates SET nome = 'amzprime - Notificação de Segurança' WHERE corpo_html = 'amazon-notificacao-seguranca';");
            migrationBuilder.Sql("UPDATE templates SET nome = 'Microsft 365 - Expiração de Senha' WHERE corpo_html = 'microcorp-expiracao-senha';");

            // Remetente (vira o NOME de exibição do header From no Gmail) e Assunto: linhas
            // legadas podem carregar a marca REAL em remetente_nome (ex.: "HBO Max", "Netflix").
            // Reescreve pela paródia canônica, casando pelo slug estável em corpo_html.
            migrationBuilder.Sql("UPDATE templates SET remetente_nome = 'bho MAX', assunto = 'Seu link para alteração de senha solicitado' WHERE corpo_html = 'hbomax-redefinicao-senha';");
            migrationBuilder.Sql("UPDATE templates SET remetente_nome = 'NetsFlix', assunto = 'Ação necessária: faltam dados da conta' WHERE corpo_html = 'netflix-atualizacao-cobranca';");
            migrationBuilder.Sql("UPDATE templates SET remetente_nome = 'amzprime', assunto = 'Atualização de Cadastro' WHERE corpo_html = 'amazon-notificacao-geral';");
            migrationBuilder.Sql("UPDATE templates SET remetente_nome = 'amzprime', assunto = 'Alerta de segurança: atividade incomum na sua conta' WHERE corpo_html = 'amazon-notificacao-seguranca';");
            migrationBuilder.Sql("UPDATE templates SET remetente_nome = 'Microsft 365', assunto = 'Ação Necessária: Expiração de Senha' WHERE corpo_html = 'microcorp-expiracao-senha';");

            // Páginas falsas (phishing_pages.html_captura = id da landing).
            migrationBuilder.Sql("UPDATE phishing_pages SET nome = 'bho MAX - Redefinição de Senha' WHERE html_captura = 'hbomax-redefinicao-senha';");
            migrationBuilder.Sql("UPDATE phishing_pages SET nome = 'NetsFlix - Acesse sua conta' WHERE html_captura = 'netflix-login';");
            migrationBuilder.Sql("UPDATE phishing_pages SET nome = 'amzprime - Alterar Senha' WHERE html_captura = 'amazon-login';");
            migrationBuilder.Sql("UPDATE phishing_pages SET nome = 'Microsft 365 - Entrar na conta' WHERE html_captura = 'microcorp-login';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sem reversão: restaurar os nomes de marca REAL é justamente o que queremos
            // evitar (compliance de IP). Data-fix intencionalmente irreversível.
        }
    }
}
