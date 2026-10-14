# Integridade do MVP

## Banco e fluxos implementados

- Campanhas são editáveis somente em Rascunho. A exclusão também exige ausência de logs e entregas; conflitos retornam 409 com mensagem tratada.
- Alvos e recursos vinculados não podem ser excluídos. As FKs para alvos usam RESTRICT; a purga completa tem procedimento separado, transacional e explícito.
- E-mail normalizado (`Trim().ToLowerInvariant()`) é obrigatório e único por tenant. O DbContext normaliza toda gravação, inclusive fora do controller; DTOs de entrada não aceitam tenant, navegações ou campos internos.
- Escritas HTTP de Targets/Campaigns/Templates/PhishingPages/EducationalPages usam transação curta e `SELECT ... FOR UPDATE` do registro do tenant. O lock PostgreSQL coordena múltiplas instâncias e serializa cota, ativação e alterações. Empresas diferentes bloqueiam registros diferentes. O transporte de e-mail continua no worker.
- As PKs UUID foram preservadas. Chaves alternativas `(tenant_id, id)` sustentam FKs compostas em campanha → modelos/páginas, logs/entregas → campanha/alvo e sessão → administrador.
- `campaign_target` é uma entidade explícita, com tenant, filtro global e FKs compostas para ambos os lados. Logs e entregas também precisam referenciar um participante dessa campanha/tenant; uma gravação direta por SQL não pode inventar uma participação.

A escolha do lock por tenant substitui a proposta inicial de Serializable/retry de 40001: todos os caminhos HTTP que criam alvos passam pelo mesmo limite transacional, consultam cota/plano depois do lock e são testados com a última vaga concorrente. Não é uma trava em memória. Novos endpoints de escrita devem participar dessa mesma coordenação.

## Snapshots na ativação

Antes de sair de Rascunho, a campanha captura, na mesma transação, nome/e-mail/departamento de cada destinatário, remetente, assunto/corpo do e-mail, página falsa e conteúdo/configuração do treinamento. IDs oficiais são materializados em HTML/JSON embutidos; retries não resolvem um catálogo atualizado para substituir esse conteúdo. Campos capturados são imutáveis também em SaveChanges do DbContext; status e controle de entregas continuam atualizáveis.

Worker, composição e transportes SMTP/API usam identidade e conteúdo capturados. Dashboards agrupam pelo departamento do participante congelado. As páginas públicas exigem o token assinado do par campanha/alvo e carregam o snapshot; o serviço de capability descobre apenas o tenant por SQL escalar parametrizado, abre um escopo temporário e mantém os filtros globais ativos durante a operação. Não desabilita filtros nas rotas públicas da campanha.

Com isso, o cadastro atual do alvo e os catálogos podem ser editados para campanhas futuras. A campanha ativada continua estruturalmente imutável. Recursos ligados a uma campanha antiga incompleta permanecem protegidos até existir snapshot utilizável. O frontend mostra os destinatários históricos em detalhes somente leitura e trata conflitos via notificações.

Coleta encerrada (`Finalizada` ou `DataFim` vencida) não grava novos eventos. O pixel ainda responde GIF e os links podem apresentar a experiência educativa, preservando a navegação sem reabrir os indicadores.

## Dados legados e migrations

1. `20261003015038_ProtectMvpCampaignHistoryAndTargetUniqueness`: backfill normalizado, índice único e RESTRICT para alvos.
2. `20261003030830_EnforceTenantRelationshipsAndCampaignSnapshots`: auditoria, escopo da associação, snapshots disponíveis, chaves alternativas e FKs compostas.

O segundo upgrade é atômico: esquema, backfill e constraints entram juntos para que a liberação da edição nunca observe um histórico parcialmente convertido. As migrations não apagam registros para satisfazer constraints. Duplicidades, vínculos incompatíveis e histórico sem participante interrompem o upgrade e preservam a transação anterior. IDs e linhas compatíveis são mantidos.

Campanhas antigas fora de Rascunho, ou com logs/entregas, recebem o cadastro/conteúdo **disponível na migração**, marcado `is_legacy_snapshot=true`. A data de captura é a data da migração. Não se afirma que esse departamento, remetente ou HTML corresponde ao que existia no envio original. O frontend e o dashboard identificam esse legado. Sem remetente conhecido, a retomada legada usa a configuração de transporte atual; não há informação para reconstruir uma identidade antiga desconhecida.

Os catálogos oficiais usados no backfill são cópias literais congeladas na migration histórica. Não regenerar esse arquivo quando mudar o catálogo atual. HTML legado é higienizado ao servir/enviar; novos snapshots passam pela allow-list antes de persistir. A sanitização preserva somente marcadores de link completos conhecidos, CIDs homologados e primitivas SVG seguras, removendo esquemas executáveis, scripts e handlers.

A normalização legada trata espaços Unicode, ASCII independente de locale e U+0130. Os testes comparam o resultado ao runtime. Caracteres internacionais incomuns ainda exigem conferência entre as tabelas Unicode do PostgreSQL e do .NET.

## Aplicação em uma instalação existente

O backend aplica migrations no startup. Antes de iniciar a nova versão com dados reais: parar API/workers, criar backup, comprovar restauração em ambiente de teste, executar `scripts/check-mvp-integrity.sql`, revisar os conflitos e testar o upgrade na cópia. Não inventar participações nem mesclar históricos automaticamente. Medir duração e planejar a janela: backfill e validação dos novos índices/FKs bloqueiam tabelas proporcionalmente ao tamanho da base.

Na validação inicial, o upgrade foi aplicado somente em PostgreSQL descartável. Em 03/10/2026, após autorização do usuário, o banco local foi atualizado: backup privado completo, restauração isolada comprovada, diagnóstico sem conflitos e fingerprints dos registros originais preservadas. As duas migrations foram aplicadas e a API retomou saudável com a imagem atualizada. Evidências e recuperação estão em `.backups/mvp-before-migration-20261003-005642-e5ed08/`.

O Down da segunda migration remove os snapshots e a proteção adicional; isso perde as capturas e pode tornar alterações recentes de cadastro incompatíveis com o comportamento da versão antiga. Para rollback operacional, usar o backup conferido; não prometer reversão histórica sem perda.

## Gestão, cota e purga

Não foi introduzido arquivamento/exclusão lógica. Cada alvo do cadastro atual participa da cota e seu e-mail atual é exclusivo por tenant. Alterar o e-mail libera o endereço anterior para outro cadastro; snapshots antigos continuam registrando o destinatário original. Apenas alvos sem vínculos podem ser excluídos pela API. Arquivar um alvo mantendo histórico exigiria definir recuperação, cota e reutilização de endereço; a decisão do MVP é manter este contrato explícito.

`scripts/purge-tenant.sql` usa UUID parametrizado, rollback por padrão e commit somente com `apply=true`. A ordem e as condições estão em [MVP-MAINTENANCE.md](MVP-MAINTENANCE.md). Não há endpoint de purga. A API e todos os workers devem estar parados para evitar envios externos durante manutenção. As chaves globais de Data Protection e os dados de outras empresas são preservados.

## Validação

`./run-mvp-integration-tests.ps1` cria PostgreSQL descartável com senha aleatória e porta interna 5433, publicado somente em loopback (55433 por padrão), roda todos os testes backend e remove somente seu container/volume. Não usa o `.env` do projeto. `dotnet test` sem `PHISHGUARD_TEST_POSTGRES` ignora os testes PostgreSQL; SQLite não comprova xmin/locks/SQLSTATE PostgreSQL.

A cobertura inclui upgrade legado válido e rollback em conflito, escrita cross-tenant por SQL, participação obrigatória, filtros da associação e do tracking público, imutabilidade dos snapshots, cota concorrente, exclusões, envio/retry congelado, janela de coleta e sanitização hostil. A purga executada nos bancos de teste confirma rollback/commit e preservação de outro tenant.

Resultados finais estão registrados em `MVP-VALIDATION.md`. O frontend foi verificado com Vitest completo e build TypeScript/Vite. O diagrama atualizado está em `docs/diagrama-fisico.md`, acompanhado de Mermaid e DBML.

## Limites conhecidos

Não há RLS PostgreSQL. A imutabilidade de snapshot em SaveChanges não impede um administrador com acesso SQL de alterar colunas diretamente. As FKs compostas continuam protegendo a integridade referencial nesse acesso.

O HTML/JSON e as referências de imagem são congelados; bytes de logos CID e imagens externas não são versionados por campanha. Uma atualização desses arquivos ou do servidor remoto pode alterar o visual de uma retomada. Perda de histórico anterior não é recuperada por estas migrations.

O pixel mede uma requisição de imagem, sujeito ao comportamento do cliente/proxy de e-mail. SMTP entrega a mensagem e não torna localhost acessível ao proxy do Gmail. Esta entrega preserva os testes HTTP do pixel e não promete detectar abertura física em clientes que não solicitam a imagem.
