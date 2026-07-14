using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhishGuard.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ApplySnakeCaseNomenclaturePattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Administradores_Tenants_TenantId",
                table: "Administradores");

            migrationBuilder.DropForeignKey(
                name: "FK_Alvos_Tenants_TenantId",
                table: "Alvos");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_EducationalPages_EducationalPageId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_PhishingPages_LandingPageId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Templates_EmailTemplateId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Tenants_TenantId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignTarget_Alvos_TargetsId",
                table: "CampaignTarget");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignTarget_Campaigns_CampaignsId",
                table: "CampaignTarget");

            migrationBuilder.DropForeignKey(
                name: "FK_EducationalPages_Tenants_TenantId",
                table: "EducationalPages");

            migrationBuilder.DropForeignKey(
                name: "FK_PhishingPages_Tenants_TenantId",
                table: "PhishingPages");

            migrationBuilder.DropForeignKey(
                name: "FK_simulations_logs_Alvos_target_id",
                table: "simulations_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_simulations_logs_Campaigns_campaign_id",
                table: "simulations_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_simulations_logs_Tenants_tenant_id",
                table: "simulations_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_smtp_configs_Tenants_TenantId",
                table: "smtp_configs");

            migrationBuilder.DropForeignKey(
                name: "FK_Templates_Tenants_TenantId",
                table: "Templates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tenants",
                table: "Tenants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Templates",
                table: "Templates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_smtp_configs",
                table: "smtp_configs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_simulations_logs",
                table: "simulations_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Campaigns",
                table: "Campaigns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Alvos",
                table: "Alvos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Administradores",
                table: "Administradores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhishingPages",
                table: "PhishingPages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EducationalPages",
                table: "EducationalPages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CampaignTarget",
                table: "CampaignTarget");

            migrationBuilder.RenameTable(
                name: "Tenants",
                newName: "tenants");

            migrationBuilder.RenameTable(
                name: "Templates",
                newName: "templates");

            migrationBuilder.RenameTable(
                name: "Campaigns",
                newName: "campaigns");

            migrationBuilder.RenameTable(
                name: "Alvos",
                newName: "alvos");

            migrationBuilder.RenameTable(
                name: "Administradores",
                newName: "administradores");

            migrationBuilder.RenameTable(
                name: "PhishingPages",
                newName: "phishing_pages");

            migrationBuilder.RenameTable(
                name: "EducationalPages",
                newName: "educational_pages");

            migrationBuilder.RenameTable(
                name: "CampaignTarget",
                newName: "campaign_target");

            migrationBuilder.RenameColumn(
                name: "Plano",
                table: "tenants",
                newName: "plano");

            migrationBuilder.RenameColumn(
                name: "Cnpj",
                table: "tenants",
                newName: "cnpj");

            migrationBuilder.RenameColumn(
                name: "Ativo",
                table: "tenants",
                newName: "ativo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tenants",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "NomeEmpresa",
                table: "tenants",
                newName: "nome_empresa");

            migrationBuilder.RenameColumn(
                name: "CriadoEm",
                table: "tenants",
                newName: "criado_em");

            migrationBuilder.RenameIndex(
                name: "IX_Tenants_Cnpj",
                table: "tenants",
                newName: "ix_tenants_cnpj");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "templates",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "Assunto",
                table: "templates",
                newName: "assunto");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "templates",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "templates",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "RemetenteNome",
                table: "templates",
                newName: "remetente_nome");

            migrationBuilder.RenameColumn(
                name: "RemetenteEmail",
                table: "templates",
                newName: "remetente_email");

            migrationBuilder.RenameColumn(
                name: "CriadoEm",
                table: "templates",
                newName: "criado_em");

            migrationBuilder.RenameColumn(
                name: "CorpoHtml",
                table: "templates",
                newName: "corpo_html");

            migrationBuilder.RenameIndex(
                name: "IX_Templates_TenantId",
                table: "templates",
                newName: "ix_templates_tenant_id");

            migrationBuilder.RenameColumn(
                name: "Usuario",
                table: "smtp_configs",
                newName: "usuario");

            migrationBuilder.RenameColumn(
                name: "Senha",
                table: "smtp_configs",
                newName: "senha");

            migrationBuilder.RenameColumn(
                name: "Porta",
                table: "smtp_configs",
                newName: "porta");

            migrationBuilder.RenameColumn(
                name: "Host",
                table: "smtp_configs",
                newName: "host");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "smtp_configs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "smtp_configs",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_smtp_configs_TenantId",
                table: "smtp_configs",
                newName: "ix_smtp_configs_tenant_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "simulations_logs",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_simulations_logs_tenant_id",
                table: "simulations_logs",
                newName: "ix_simulations_logs_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_simulations_logs_target_id",
                table: "simulations_logs",
                newName: "ix_simulations_logs_target_id");

            migrationBuilder.RenameIndex(
                name: "IX_simulations_logs_campaign_id",
                table: "simulations_logs",
                newName: "ix_simulations_logs_campaign_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "campaigns",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "campaigns",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "campaigns",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "NomeCampanha",
                table: "campaigns",
                newName: "nome_campanha");

            migrationBuilder.RenameColumn(
                name: "LandingPageId",
                table: "campaigns",
                newName: "landing_page_id");

            migrationBuilder.RenameColumn(
                name: "EmailTemplateId",
                table: "campaigns",
                newName: "email_template_id");

            migrationBuilder.RenameColumn(
                name: "EducationalPageId",
                table: "campaigns",
                newName: "educational_page_id");

            migrationBuilder.RenameColumn(
                name: "DataInicio",
                table: "campaigns",
                newName: "data_inicio");

            migrationBuilder.RenameColumn(
                name: "DataFim",
                table: "campaigns",
                newName: "data_fim");

            migrationBuilder.RenameColumn(
                name: "CriadoEm",
                table: "campaigns",
                newName: "criado_em");

            migrationBuilder.RenameIndex(
                name: "IX_Campaigns_TenantId",
                table: "campaigns",
                newName: "ix_campaigns_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_Campaigns_LandingPageId",
                table: "campaigns",
                newName: "ix_campaigns_landing_page_id");

            migrationBuilder.RenameIndex(
                name: "IX_Campaigns_EmailTemplateId",
                table: "campaigns",
                newName: "ix_campaigns_email_template_id");

            migrationBuilder.RenameIndex(
                name: "IX_Campaigns_EducationalPageId",
                table: "campaigns",
                newName: "ix_campaigns_educational_page_id");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "alvos",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "alvos",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Departamento",
                table: "alvos",
                newName: "departamento");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "alvos",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "alvos",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_Alvos_TenantId",
                table: "alvos",
                newName: "ix_alvos_tenant_id");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "administradores",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "administradores",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "administradores",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "administradores",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "administradores",
                newName: "password_hash");

            migrationBuilder.RenameIndex(
                name: "IX_Administradores_Email",
                table: "administradores",
                newName: "ix_administradores_email");

            migrationBuilder.RenameIndex(
                name: "IX_Administradores_TenantId",
                table: "administradores",
                newName: "ix_administradores_tenant_id");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "phishing_pages",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "phishing_pages",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "phishing_pages",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "HtmlCaptura",
                table: "phishing_pages",
                newName: "html_captura");

            migrationBuilder.RenameColumn(
                name: "CriadoEm",
                table: "phishing_pages",
                newName: "criado_em");

            migrationBuilder.RenameIndex(
                name: "IX_PhishingPages_TenantId",
                table: "phishing_pages",
                newName: "ix_phishing_pages_tenant_id");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "educational_pages",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "educational_pages",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "educational_pages",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "HtmlEducacional",
                table: "educational_pages",
                newName: "html_educacional");

            migrationBuilder.RenameColumn(
                name: "CriadoEm",
                table: "educational_pages",
                newName: "criado_em");

            migrationBuilder.RenameIndex(
                name: "IX_EducationalPages_TenantId",
                table: "educational_pages",
                newName: "ix_educational_pages_tenant_id");

            migrationBuilder.RenameColumn(
                name: "TargetsId",
                table: "campaign_target",
                newName: "targets_id");

            migrationBuilder.RenameColumn(
                name: "CampaignsId",
                table: "campaign_target",
                newName: "campaigns_id");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignTarget_TargetsId",
                table: "campaign_target",
                newName: "ix_campaign_target_targets_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_tenants",
                table: "tenants",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_templates",
                table: "templates",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_smtp_configs",
                table: "smtp_configs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_simulations_logs",
                table: "simulations_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_campaigns",
                table: "campaigns",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_alvos",
                table: "alvos",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_administradores",
                table: "administradores",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_phishing_pages",
                table: "phishing_pages",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_educational_pages",
                table: "educational_pages",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_campaign_target",
                table: "campaign_target",
                columns: new[] { "campaigns_id", "targets_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_administradores_tenants_tenant_id",
                table: "administradores",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_alvos_tenants_tenant_id",
                table: "alvos",
                column: "tenant_id",
                principalTable: "tenants",
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
                name: "fk_campaigns_tenants_tenant_id",
                table: "campaigns",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_educational_pages_tenants_tenant_id",
                table: "educational_pages",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_phishing_pages_tenants_tenant_id",
                table: "phishing_pages",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_simulations_logs_alvos_target_id",
                table: "simulations_logs",
                column: "target_id",
                principalTable: "alvos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_simulations_logs_campaigns_campaign_id",
                table: "simulations_logs",
                column: "campaign_id",
                principalTable: "campaigns",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_simulations_logs_tenants_tenant_id",
                table: "simulations_logs",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_smtp_configs_tenants_tenant_id",
                table: "smtp_configs",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_templates_tenants_tenant_id",
                table: "templates",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_administradores_tenants_tenant_id",
                table: "administradores");

            migrationBuilder.DropForeignKey(
                name: "fk_alvos_tenants_tenant_id",
                table: "alvos");

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
                name: "fk_campaigns_tenants_tenant_id",
                table: "campaigns");

            migrationBuilder.DropForeignKey(
                name: "fk_educational_pages_tenants_tenant_id",
                table: "educational_pages");

            migrationBuilder.DropForeignKey(
                name: "fk_phishing_pages_tenants_tenant_id",
                table: "phishing_pages");

            migrationBuilder.DropForeignKey(
                name: "fk_simulations_logs_alvos_target_id",
                table: "simulations_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_simulations_logs_campaigns_campaign_id",
                table: "simulations_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_simulations_logs_tenants_tenant_id",
                table: "simulations_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_smtp_configs_tenants_tenant_id",
                table: "smtp_configs");

            migrationBuilder.DropForeignKey(
                name: "fk_templates_tenants_tenant_id",
                table: "templates");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tenants",
                table: "tenants");

            migrationBuilder.DropPrimaryKey(
                name: "pk_templates",
                table: "templates");

            migrationBuilder.DropPrimaryKey(
                name: "pk_smtp_configs",
                table: "smtp_configs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_simulations_logs",
                table: "simulations_logs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_campaigns",
                table: "campaigns");

            migrationBuilder.DropPrimaryKey(
                name: "pk_alvos",
                table: "alvos");

            migrationBuilder.DropPrimaryKey(
                name: "pk_administradores",
                table: "administradores");

            migrationBuilder.DropPrimaryKey(
                name: "pk_phishing_pages",
                table: "phishing_pages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_educational_pages",
                table: "educational_pages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_campaign_target",
                table: "campaign_target");

            migrationBuilder.RenameTable(
                name: "tenants",
                newName: "Tenants");

            migrationBuilder.RenameTable(
                name: "templates",
                newName: "Templates");

            migrationBuilder.RenameTable(
                name: "campaigns",
                newName: "Campaigns");

            migrationBuilder.RenameTable(
                name: "alvos",
                newName: "Alvos");

            migrationBuilder.RenameTable(
                name: "administradores",
                newName: "Administradores");

            migrationBuilder.RenameTable(
                name: "phishing_pages",
                newName: "PhishingPages");

            migrationBuilder.RenameTable(
                name: "educational_pages",
                newName: "EducationalPages");

            migrationBuilder.RenameTable(
                name: "campaign_target",
                newName: "CampaignTarget");

            migrationBuilder.RenameColumn(
                name: "plano",
                table: "Tenants",
                newName: "Plano");

            migrationBuilder.RenameColumn(
                name: "cnpj",
                table: "Tenants",
                newName: "Cnpj");

            migrationBuilder.RenameColumn(
                name: "ativo",
                table: "Tenants",
                newName: "Ativo");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Tenants",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "nome_empresa",
                table: "Tenants",
                newName: "NomeEmpresa");

            migrationBuilder.RenameColumn(
                name: "criado_em",
                table: "Tenants",
                newName: "CriadoEm");

            migrationBuilder.RenameIndex(
                name: "ix_tenants_cnpj",
                table: "Tenants",
                newName: "IX_Tenants_Cnpj");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "Templates",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "assunto",
                table: "Templates",
                newName: "Assunto");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Templates",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "Templates",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "remetente_nome",
                table: "Templates",
                newName: "RemetenteNome");

            migrationBuilder.RenameColumn(
                name: "remetente_email",
                table: "Templates",
                newName: "RemetenteEmail");

            migrationBuilder.RenameColumn(
                name: "criado_em",
                table: "Templates",
                newName: "CriadoEm");

            migrationBuilder.RenameColumn(
                name: "corpo_html",
                table: "Templates",
                newName: "CorpoHtml");

            migrationBuilder.RenameIndex(
                name: "ix_templates_tenant_id",
                table: "Templates",
                newName: "IX_Templates_TenantId");

            migrationBuilder.RenameColumn(
                name: "usuario",
                table: "smtp_configs",
                newName: "Usuario");

            migrationBuilder.RenameColumn(
                name: "senha",
                table: "smtp_configs",
                newName: "Senha");

            migrationBuilder.RenameColumn(
                name: "porta",
                table: "smtp_configs",
                newName: "Porta");

            migrationBuilder.RenameColumn(
                name: "host",
                table: "smtp_configs",
                newName: "Host");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "smtp_configs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "smtp_configs",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "ix_smtp_configs_tenant_id",
                table: "smtp_configs",
                newName: "IX_smtp_configs_TenantId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "simulations_logs",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "ix_simulations_logs_tenant_id",
                table: "simulations_logs",
                newName: "IX_simulations_logs_tenant_id");

            migrationBuilder.RenameIndex(
                name: "ix_simulations_logs_target_id",
                table: "simulations_logs",
                newName: "IX_simulations_logs_target_id");

            migrationBuilder.RenameIndex(
                name: "ix_simulations_logs_campaign_id",
                table: "simulations_logs",
                newName: "IX_simulations_logs_campaign_id");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Campaigns",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Campaigns",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "Campaigns",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "nome_campanha",
                table: "Campaigns",
                newName: "NomeCampanha");

            migrationBuilder.RenameColumn(
                name: "landing_page_id",
                table: "Campaigns",
                newName: "LandingPageId");

            migrationBuilder.RenameColumn(
                name: "email_template_id",
                table: "Campaigns",
                newName: "EmailTemplateId");

            migrationBuilder.RenameColumn(
                name: "educational_page_id",
                table: "Campaigns",
                newName: "EducationalPageId");

            migrationBuilder.RenameColumn(
                name: "data_inicio",
                table: "Campaigns",
                newName: "DataInicio");

            migrationBuilder.RenameColumn(
                name: "data_fim",
                table: "Campaigns",
                newName: "DataFim");

            migrationBuilder.RenameColumn(
                name: "criado_em",
                table: "Campaigns",
                newName: "CriadoEm");

            migrationBuilder.RenameIndex(
                name: "ix_campaigns_tenant_id",
                table: "Campaigns",
                newName: "IX_Campaigns_TenantId");

            migrationBuilder.RenameIndex(
                name: "ix_campaigns_landing_page_id",
                table: "Campaigns",
                newName: "IX_Campaigns_LandingPageId");

            migrationBuilder.RenameIndex(
                name: "ix_campaigns_email_template_id",
                table: "Campaigns",
                newName: "IX_Campaigns_EmailTemplateId");

            migrationBuilder.RenameIndex(
                name: "ix_campaigns_educational_page_id",
                table: "Campaigns",
                newName: "IX_Campaigns_EducationalPageId");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "Alvos",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Alvos",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "departamento",
                table: "Alvos",
                newName: "Departamento");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Alvos",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "Alvos",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "ix_alvos_tenant_id",
                table: "Alvos",
                newName: "IX_Alvos_TenantId");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "Administradores",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Administradores",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Administradores",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "Administradores",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Administradores",
                newName: "PasswordHash");

            migrationBuilder.RenameIndex(
                name: "ix_administradores_email",
                table: "Administradores",
                newName: "IX_Administradores_Email");

            migrationBuilder.RenameIndex(
                name: "ix_administradores_tenant_id",
                table: "Administradores",
                newName: "IX_Administradores_TenantId");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "PhishingPages",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PhishingPages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "PhishingPages",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "html_captura",
                table: "PhishingPages",
                newName: "HtmlCaptura");

            migrationBuilder.RenameColumn(
                name: "criado_em",
                table: "PhishingPages",
                newName: "CriadoEm");

            migrationBuilder.RenameIndex(
                name: "ix_phishing_pages_tenant_id",
                table: "PhishingPages",
                newName: "IX_PhishingPages_TenantId");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "EducationalPages",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "EducationalPages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "EducationalPages",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "html_educacional",
                table: "EducationalPages",
                newName: "HtmlEducacional");

            migrationBuilder.RenameColumn(
                name: "criado_em",
                table: "EducationalPages",
                newName: "CriadoEm");

            migrationBuilder.RenameIndex(
                name: "ix_educational_pages_tenant_id",
                table: "EducationalPages",
                newName: "IX_EducationalPages_TenantId");

            migrationBuilder.RenameColumn(
                name: "targets_id",
                table: "CampaignTarget",
                newName: "TargetsId");

            migrationBuilder.RenameColumn(
                name: "campaigns_id",
                table: "CampaignTarget",
                newName: "CampaignsId");

            migrationBuilder.RenameIndex(
                name: "ix_campaign_target_targets_id",
                table: "CampaignTarget",
                newName: "IX_CampaignTarget_TargetsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tenants",
                table: "Tenants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Templates",
                table: "Templates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_smtp_configs",
                table: "smtp_configs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_simulations_logs",
                table: "simulations_logs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Campaigns",
                table: "Campaigns",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Alvos",
                table: "Alvos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Administradores",
                table: "Administradores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhishingPages",
                table: "PhishingPages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EducationalPages",
                table: "EducationalPages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CampaignTarget",
                table: "CampaignTarget",
                columns: new[] { "CampaignsId", "TargetsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Administradores_Tenants_TenantId",
                table: "Administradores",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Alvos_Tenants_TenantId",
                table: "Alvos",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_EducationalPages_EducationalPageId",
                table: "Campaigns",
                column: "EducationalPageId",
                principalTable: "EducationalPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_PhishingPages_LandingPageId",
                table: "Campaigns",
                column: "LandingPageId",
                principalTable: "PhishingPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Templates_EmailTemplateId",
                table: "Campaigns",
                column: "EmailTemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Tenants_TenantId",
                table: "Campaigns",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignTarget_Alvos_TargetsId",
                table: "CampaignTarget",
                column: "TargetsId",
                principalTable: "Alvos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignTarget_Campaigns_CampaignsId",
                table: "CampaignTarget",
                column: "CampaignsId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalPages_Tenants_TenantId",
                table: "EducationalPages",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhishingPages_Tenants_TenantId",
                table: "PhishingPages",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_simulations_logs_Alvos_target_id",
                table: "simulations_logs",
                column: "target_id",
                principalTable: "Alvos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_simulations_logs_Campaigns_campaign_id",
                table: "simulations_logs",
                column: "campaign_id",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_simulations_logs_Tenants_tenant_id",
                table: "simulations_logs",
                column: "tenant_id",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_smtp_configs_Tenants_TenantId",
                table: "smtp_configs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Templates_Tenants_TenantId",
                table: "Templates",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
