-- MANUTENÇÃO EXPLÍCITA: apagar permanentemente um tenant e seu histórico.
-- Fazer backup testado e parar backend/workers antes de aplicar.
-- psql "$CONNECTION_STRING" -v tenant_id=UUID -f scripts/purge-tenant.sql
-- O comando acima SEMPRE desfaz as exclusões. Para aplicar, acrescentar -v apply=true.
-- As variáveis :'tenant_id' são escapadas pelo psql como literais e validadas como uuid.
-- Chaves Data Protection são compartilhadas e não participam da purga.
\set ON_ERROR_STOP on
\if :{?tenant_id}
\else
  \echo 'Informe -v tenant_id=UUID. Nenhuma alteração executada.'
  \quit 1
\endif
\if :{?apply}
\else
  \set apply false
\endif

BEGIN;
SET LOCAL lock_timeout = '5s';
SET LOCAL statement_timeout = '5min';

-- Mesmo registro usado pela coordenação das escritas HTTP; os workers devem estar parados.
SELECT id FROM tenants WHERE id = :'tenant_id'::uuid FOR UPDATE;

-- A ordem preserva as FKs de participação, histórico, recursos e autenticação.
DELETE FROM simulations_logs WHERE tenant_id = :'tenant_id'::uuid;
DELETE FROM campaign_deliveries WHERE tenant_id = :'tenant_id'::uuid;
DELETE FROM campaign_target WHERE tenant_id = :'tenant_id'::uuid;
DELETE FROM campaigns WHERE tenant_id = :'tenant_id'::uuid;
DELETE FROM auth_sessions WHERE tenant_id = :'tenant_id'::uuid;
DELETE FROM administradores WHERE tenant_id = :'tenant_id'::uuid;
DELETE FROM smtp_configs WHERE tenant_id = :'tenant_id'::uuid;
DELETE FROM alvos WHERE tenant_id = :'tenant_id'::uuid;
DELETE FROM templates WHERE tenant_id = :'tenant_id'::uuid;
DELETE FROM phishing_pages WHERE tenant_id = :'tenant_id'::uuid;
DELETE FROM educational_pages WHERE tenant_id = :'tenant_id'::uuid;
DELETE FROM tenants WHERE id = :'tenant_id'::uuid;

\if :apply
  COMMIT;
  \echo 'Purga confirmada para o tenant informado.'
\else
  ROLLBACK;
  \echo 'Simulação concluída: todas as exclusões foram desfeitas. Para aplicar: -v apply=true.'
\endif
