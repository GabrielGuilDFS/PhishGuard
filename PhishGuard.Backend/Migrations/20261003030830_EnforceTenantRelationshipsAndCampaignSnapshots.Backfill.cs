using Microsoft.EntityFrameworkCore.Migrations;

namespace PhishGuard.Backend.Migrations;

public partial class EnforceTenantRelationshipsAndCampaignSnapshots
{
    private static void ValidateLegacyRelationships(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT c.id FROM campaigns c
                    LEFT JOIN templates t ON t.id = c.email_template_id
                    LEFT JOIN phishing_pages p ON p.id = c.landing_page_id
                    LEFT JOIN educational_pages e ON e.id = c.educational_page_id
                    WHERE t.id IS NULL OR p.id IS NULL OR e.id IS NULL
                       OR c.tenant_id <> t.tenant_id OR c.tenant_id <> p.tenant_id OR c.tenant_id <> e.tenant_id
                    UNION ALL
                    SELECT ct.campaigns_id FROM campaign_target ct
                    LEFT JOIN campaigns c ON c.id = ct.campaigns_id
                    LEFT JOIN alvos a ON a.id = ct.targets_id
                    WHERE c.id IS NULL OR a.id IS NULL OR c.tenant_id <> a.tenant_id
                    UNION ALL
                    SELECT l.id FROM simulations_logs l
                    LEFT JOIN campaigns c ON c.id = l.campaign_id
                    LEFT JOIN alvos a ON a.id = l.target_id
                    LEFT JOIN campaign_target ct ON ct.campaigns_id = l.campaign_id AND ct.targets_id = l.target_id
                    WHERE c.id IS NULL OR a.id IS NULL OR l.tenant_id <> c.tenant_id OR l.tenant_id <> a.tenant_id OR ct.campaigns_id IS NULL
                    UNION ALL
                    SELECT d.id FROM campaign_deliveries d
                    LEFT JOIN campaigns c ON c.id = d.campaign_id
                    LEFT JOIN alvos a ON a.id = d.target_id
                    LEFT JOIN campaign_target ct ON ct.campaigns_id = d.campaign_id AND ct.targets_id = d.target_id
                    WHERE c.id IS NULL OR a.id IS NULL OR d.tenant_id <> c.tenant_id OR d.tenant_id <> a.tenant_id OR ct.campaigns_id IS NULL
                    UNION ALL
                    SELECT s.id FROM auth_sessions s LEFT JOIN administradores a ON a.id = s.administrator_id
                    WHERE a.id IS NULL OR s.tenant_id <> a.tenant_id
                ) THEN
                    RAISE EXCEPTION USING ERRCODE = '23514',
                        MESSAGE = 'Integridade MVP: referências ausentes, entre tenants ou histórico sem participante. Execute scripts/check-mvp-integrity.sql e revise os conflitos antes de migrar.';
                END IF;
            END $$;
            """);
    }

    private static void BackfillLegacySnapshots(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE campaign_target ct SET tenant_id = c.tenant_id
            FROM campaigns c WHERE c.id = ct.campaigns_id;

            -- These are captures of the data available NOW, not reconstructed history.
            UPDATE campaigns c SET
                template_name_snapshot = t.nome,
                email_subject_snapshot = t.assunto,
                email_body_snapshot = t.corpo_html,
                sender_name_snapshot = COALESCE(NULLIF((SELECT s.sender_name FROM smtp_configs s WHERE s.tenant_id = c.tenant_id), ''), t.remetente_nome),
                sender_email_snapshot = COALESCE(NULLIF((SELECT CASE WHEN s.provider_type = 1 THEN s.sender_email ELSE s.usuario END FROM smtp_configs s WHERE s.tenant_id = c.tenant_id), ''), NULLIF(t.remetente_email, '')),
                landing_name_snapshot = p.nome,
                landing_html_snapshot = p.html_captura,
                educational_name_snapshot = e.nome,
                educational_html_snapshot = e.html_educacional,
                snapshot_captured_at_utc = CURRENT_TIMESTAMP,
                is_legacy_snapshot = true
            FROM templates t, phishing_pages p, educational_pages e
            WHERE c.email_template_id = t.id AND c.landing_page_id = p.id AND c.educational_page_id = e.id
              AND (c.status <> 'Rascunho'
                   OR EXISTS (SELECT 1 FROM simulations_logs l WHERE l.campaign_id = c.id)
                   OR EXISTS (SELECT 1 FROM campaign_deliveries d WHERE d.campaign_id = c.id));

            UPDATE campaign_target ct SET
                target_name_snapshot = a.nome,
                target_email_snapshot = a.email,
                department_snapshot = a.departamento,
                snapshot_captured_at_utc = c.snapshot_captured_at_utc,
                is_legacy_snapshot = true
            FROM campaigns c, alvos a
            WHERE c.id = ct.campaigns_id AND a.id = ct.targets_id AND c.snapshot_captured_at_utc IS NOT NULL;
            """);
    }
}
