-- Diagnóstico somente leitura. Executar com acesso administrativo autorizado.
-- Retorna chaves para revisão; não remove nem altera registros.
-- Compatível com a associação legada sem tenant_id: o tenant vem da campanha.
BEGIN READ ONLY;
WITH normalized AS (
    SELECT id, tenant_id, (
                    SELECT string_agg(CASE
                        WHEN ascii(part) BETWEEN 65 AND 90 THEN chr(ascii(part) + 32)
                        WHEN ascii(part) = 304 THEN part
                        ELSE lower(part) END, '' ORDER BY ordinal)
                    FROM regexp_split_to_table(
                        btrim(email, U&'\0009\000A\000B\000C\000D\0020\0085\00A0\1680\2000\2001\2002\2003\2004\2005\2006\2007\2008\2009\200A\2028\2029\202F\205F\3000'),
                        '') WITH ORDINALITY AS letters(part, ordinal)
    ) AS email_normalizado FROM alvos
)
SELECT tenant_id, email_normalizado, count(*) AS quantidade,
       array_agg(id ORDER BY id) AS alvos
FROM normalized
GROUP BY tenant_id, email_normalizado HAVING count(*) > 1;

-- IDs revisáveis de órfãos, referências cross-tenant e histórico sem participação.
-- LEFT JOIN é deliberado: INNER JOIN esconderia recursos/pessoas já inexistentes.
WITH inconsistencias AS (
    SELECT 'campaign_template' AS relacao, c.tenant_id, c.id AS registro_id,
           c.email_template_id AS referencia_id,
           CASE WHEN t.id IS NULL THEN 'referencia_ausente' ELSE 'tenant_divergente' END AS motivo
    FROM campaigns c LEFT JOIN templates t ON t.id = c.email_template_id
    WHERE t.id IS NULL OR c.tenant_id <> t.tenant_id
    UNION ALL
    SELECT 'campaign_phishing_page', c.tenant_id, c.id, c.landing_page_id,
           CASE WHEN p.id IS NULL THEN 'referencia_ausente' ELSE 'tenant_divergente' END
    FROM campaigns c LEFT JOIN phishing_pages p ON p.id = c.landing_page_id
    WHERE p.id IS NULL OR c.tenant_id <> p.tenant_id
    UNION ALL
    SELECT 'campaign_educational_page', c.tenant_id, c.id, c.educational_page_id,
           CASE WHEN e.id IS NULL THEN 'referencia_ausente' ELSE 'tenant_divergente' END
    FROM campaigns c LEFT JOIN educational_pages e ON e.id = c.educational_page_id
    WHERE e.id IS NULL OR c.tenant_id <> e.tenant_id
    UNION ALL
    SELECT 'campaign_target', c.tenant_id, ct.campaigns_id, ct.targets_id,
           CASE WHEN c.id IS NULL OR a.id IS NULL THEN 'referencia_ausente' ELSE 'tenant_divergente' END
    FROM campaign_target ct LEFT JOIN campaigns c ON c.id = ct.campaigns_id
    LEFT JOIN alvos a ON a.id = ct.targets_id
    WHERE c.id IS NULL OR a.id IS NULL OR c.tenant_id <> a.tenant_id
    UNION ALL
    SELECT 'simulation_log', l.tenant_id, l.id, l.target_id,
           CASE WHEN c.id IS NULL OR a.id IS NULL THEN 'referencia_ausente'
                WHEN l.tenant_id <> c.tenant_id OR l.tenant_id <> a.tenant_id THEN 'tenant_divergente'
                ELSE 'alvo_sem_participacao' END
    FROM simulations_logs l LEFT JOIN campaigns c ON c.id = l.campaign_id
    LEFT JOIN alvos a ON a.id = l.target_id
    LEFT JOIN campaign_target ct ON ct.campaigns_id = l.campaign_id AND ct.targets_id = l.target_id
    WHERE c.id IS NULL OR a.id IS NULL OR l.tenant_id <> c.tenant_id OR l.tenant_id <> a.tenant_id OR ct.campaigns_id IS NULL
    UNION ALL
    SELECT 'campaign_delivery', d.tenant_id, d.id, d.target_id,
           CASE WHEN c.id IS NULL OR a.id IS NULL THEN 'referencia_ausente'
                WHEN d.tenant_id <> c.tenant_id OR d.tenant_id <> a.tenant_id THEN 'tenant_divergente'
                ELSE 'alvo_sem_participacao' END
    FROM campaign_deliveries d LEFT JOIN campaigns c ON c.id = d.campaign_id
    LEFT JOIN alvos a ON a.id = d.target_id
    LEFT JOIN campaign_target ct ON ct.campaigns_id = d.campaign_id AND ct.targets_id = d.target_id
    WHERE c.id IS NULL OR a.id IS NULL OR d.tenant_id <> c.tenant_id OR d.tenant_id <> a.tenant_id OR ct.campaigns_id IS NULL
    UNION ALL
    SELECT 'auth_session', s.tenant_id, s.id, s.administrator_id,
           CASE WHEN a.id IS NULL THEN 'referencia_ausente' ELSE 'tenant_divergente' END
    FROM auth_sessions s LEFT JOIN administradores a ON a.id = s.administrator_id
    WHERE a.id IS NULL OR s.tenant_id <> a.tenant_id
)
SELECT relacao, tenant_id, registro_id, referencia_id, motivo
FROM inconsistencias ORDER BY relacao, tenant_id, registro_id;

-- Qualquer linha aqui precisa ser revisada; não sugerir novo tenant por heurística.
SELECT 'administradores' AS tabela, id FROM administradores a WHERE NOT EXISTS (SELECT 1 FROM tenants t WHERE t.id = a.tenant_id)
UNION ALL SELECT 'alvos', id FROM alvos a WHERE NOT EXISTS (SELECT 1 FROM tenants t WHERE t.id = a.tenant_id)
UNION ALL SELECT 'campaigns', id FROM campaigns c WHERE NOT EXISTS (SELECT 1 FROM tenants t WHERE t.id = c.tenant_id)
UNION ALL SELECT 'templates', id FROM templates r WHERE NOT EXISTS (SELECT 1 FROM tenants t WHERE t.id = r.tenant_id)
UNION ALL SELECT 'phishing_pages', id FROM phishing_pages r WHERE NOT EXISTS (SELECT 1 FROM tenants t WHERE t.id = r.tenant_id)
UNION ALL SELECT 'educational_pages', id FROM educational_pages r WHERE NOT EXISTS (SELECT 1 FROM tenants t WHERE t.id = r.tenant_id);

COMMIT;
