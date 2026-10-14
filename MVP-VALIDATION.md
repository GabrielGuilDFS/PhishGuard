# Validação da integridade do MVP — 03/10/2026

## Resultado final

| Verificação | Resultado |
|---|---|
| Backend completo com PostgreSQL descartável (`run-mvp-integration-tests.ps1`) | 252 aprovados, zero falhas, zero ignorados |
| Frontend completo (`npm run test:run`) | 197 aprovados em 34 arquivos, zero falhas |
| Backend Release (`dotnet build ... -c Release --no-restore`) | Êxito, zero erros e zero avisos |
| Frontend (`npm run build`, TypeScript + Vite) | Êxito, após as últimas mudanças de Campanhas |
| Snapshot EF × modelo Npgsql | Sem mudanças pendentes, confirmado nos testes de upgrade |
| Diagrama regenerado do snapshot | 13 tabelas, 120 atributos incluindo xmin, 22 FKs |
| `git diff --check` | Sem erros de whitespace |

## Evidências relevantes

- **Constraints fora da API:** tentativas de gravação SQL parametrizada cross-tenant, histórico sem participante e remoção de participante com histórico falham em SQLite/PostgreSQL. As PKs UUID e os IDs existentes são preservados.
- **Upgrade real:** migração a partir do esquema legado; referências incompatíveis e participação ausente abortam sem perda de linhas ou backfill parcial. Backfill válido mantém IDs e sinaliza o snapshot como legado. O diagnóstico SQL real também é executado nos testes.
- **Concorrência:** última vaga da cota, independência entre tenants, cancelamento e disputa entre ativação/edição são testados em PostgreSQL. A coordenação utiliza lock transacional no banco.
- **Histórico:** ativação captura destinatário/departamento/remetente e HTML/JSON real dos recursos. Cadastro atual pode mudar sem reclassificar o relatório. Retry, SMTP e cinco provedores API respeitam a identidade capturada. Rascunhos legados com histórico/captura não podem ser editados, excluídos ou ativados novamente.
- **Tracking público:** HTTP real em Kestrel/SQLite com HMAC real comprova abertura observada sem clique; token inválido e ausência de Envio não poluem métricas. O escopo público mantém filtros ativos, restaura o tenant ao terminar e rejeita capability de outro tenant autenticado. A descoberta UUID por SQL escalar também passa em PostgreSQL.
- **Coleta encerrada:** campanhas finalizadas ou com DataFim vencida não gravam novos eventos, mantendo GIF e navegação.
- **Sanitização:** URLs executáveis com placeholders, tokens parciais, CID desconhecido, handlers/scripts e SVG hostil são removidos. CIDs homologados, placeholders completos conhecidos e SVG gráfico dos cinco catálogos oficiais permanecem utilizáveis.
- **Manutenção:** os DELETEs extraídos do script de purga real são testados em transação, incluindo rollback, commit e preservação de outro tenant e das chaves globais.
- **Frontend:** campanha ativada/protegida é somente leitura, apresenta destinatários preservados e legado; páginas públicas usam snapshots assinados e tratam erros. Configurações mantêm seus seis testes aprovados. A hierarquia de títulos da landing Amazon passa na verificação de acessibilidade.

## Ambiente e limites

Os testes PostgreSQL usam container exclusivo, senha aleatória e publicação em loopback. O script remove somente o container/volume que criou. Na fase inicial de validação, nenhuma migration, diagnóstico ou purga foi executada no banco de usuários.

Em 03/10/2026, o usuário autorizou backup e aplicação das migrations no banco local. Backup completo privado em `.backups/mvp-before-migration-20261003-005642-e5ed08/`, com SHA-256 conferido e restauração isolada comprovada. O upgrade da cópia e do banco atual preservou fingerprints de todos os registros originais. Diagnósticos sem conflitos; 30 migrations aplicadas, 23 campanhas e 52 participações capturadas como legado, nenhuma FK não validada. API retomada na imagem atualizada e saudável; HTTP 200 nos healthchecks/frontend, HTTP 401 no acesso admin/proxy sem sessão, HTTP 404 para landing sem capability e GIF para pixel inválido. Nenhuma purga foi executada nem campanha real foi disparada nesta manutenção.

Antes de usar a nova versão em uma instalação existente, seguir [MVP-MAINTENANCE.md](MVP-MAINTENANCE.md): backup com restauração comprovada, diagnóstico, saneamento revisado e upgrade na cópia. O backend aplica migrations no startup.

Testes não comprovam a versão histórica do departamento/conteúdo antigo, comportamento de abertura no Gmail/Mailtrap ou disponibilidade pública de localhost. Legado é explicitamente identificado. Bytes de logos CID/imagens externas não são versionados por campanha. Imutabilidade do snapshot é aplicada pelo DbContext; acesso SQL administrativo continua exigindo governança.

Contrato completo: [MVP-INTEGRITY.md](MVP-INTEGRITY.md). Diagrama atualizado: [docs/diagrama-fisico.md](docs/diagrama-fisico.md).
