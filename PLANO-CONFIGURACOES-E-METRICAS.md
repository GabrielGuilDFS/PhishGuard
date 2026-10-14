# Configurações em ultrawide e revisão de métricas de e-mail

## Escopo e evidências

Plano elaborado a partir do código atual. Personas: Engenheiro Frontend Sênior para Configurações; AppSec & QA e Architect & SRE para rastreamento e envio. Após a instrução de implementar primeiro a responsividade, o layout de Configurações foi ajustado. A lógica de coleta de métricas permanece como analisada aqui.

A revisão cobre composição do e-mail, envio, endpoints de tracking, agregação do dashboard e apresentação dos indicadores. Os 22 testes existentes de `TrackingController`, `TrackingTokenService`, `EmailMessageComposer` e `DashboardOverviewService` passaram. Isso valida os cenários já cobertos, não reproduz todos os riscos abaixo. Não houve experimento numa conta real do Mailtrap nesta revisão.

Responsividade implementada: grids por largura útil do contêiner; envio/status e seleção/prévia de tema lado a lado quando couberem; campos Host/Porta adaptáveis; abas roláveis; remoção do limite mínimo local que causaria pressão em mobile. O limite global de 1.280 px foi preservado. Build de produção e os seis testes existentes de Settings passaram. Chromium verificou 108 combinações: três abas, dois temas, SMTP/Mailtrap e nove larguras (320 a 5.120 px), com API simulada, sem overflow da página ou do main e sem erros JavaScript. Screenshots em 375 e 1.920 px foram inspecionados. Zoom e os outros provedores permanecem na matriz de aceite proposta, sem alegação de validação nesta execução.

## 1. Plano de responsividade de Configurações

### Diagnóstico

- `src/components/PageContainer.tsx` centraliza as páginas em até 1.280 px. É um limite deliberado de leitura; margens em monitores ultrawide são esperadas.
- `src/pages/Settings.tsx` limita a aba de envio a 600 px e Aparência a 560 px, deixando parte do próprio painel vazia.
- Perfil já usa duas colunas a partir de `md`; serve de referência para as demais abas.
- Cabeçalho de envio e campos Host/Porta usam linhas fixas. Os dois seletores de transporte têm textos longos. Esses pontos precisam de adaptação também quando o navegador ultrawide está dividido ou com zoom.

### Layout proposto

Manter o `PageContainer` global em 1.280 px, MUI e padding horizontal no `AdminLayout`. Aproveitar melhor a largura interna antes de aumentar a página. Campos não devem ocupar toda a largura de um monitor de 3.440 px.

| Área | Em largura suficiente | Em largura reduzida |
| --- | --- | --- |
| Perfil | Dados pessoais e Segurança lado a lado, alinhados no topo | Uma coluna, mantendo a ordem de leitura |
| Entrega de e-mail | Formulário à esquerda e painel de status/teste à direita | Formulário e depois status, numa coluna |
| Aparência | Seleção de tema à esquerda e prévia do painel à direita | Seleção e prévia empilhadas |

Na aba de envio, usar um grid com aproximadamente 2/3 da largura para configuração e 1/3 para status, com `minmax(0, ...)`, `minWidth: 0` nos filhos e espaçamento de 24 px. Ativar duas colunas apenas quando a área útil suportar formulário de pelo menos 560 px e painel de 280 px mais espaçamento. O breakpoint deve considerar os 260 px do menu lateral e o padding; não basta olhar a resolução física do monitor.

O painel lateral deve aproveitar os dados existentes: configuração salva, credencial cadastrada (apenas indicação), alterações pendentes, disponibilidade do transporte e resultado do último teste. Não mostrar senha/API key. Separar claramente “teste do transporte” de “rastreamento público”: o primeiro não certifica o segundo.

### Etapas de implementação

1. **Estrutura da página:** organizar seções internas em componentes pequenos de Configurações, mantendo os estados e handlers existentes. Remover as limitações de 600/560 px do contêiner principal das abas; preservar limites de leitura nos blocos internos.
2. **Entrega de e-mail:** agrupar transporte, remetente, conexão e credenciais. Host/Porta lado a lado quando couberem; empilhar em telas pequenas. Campos de credencial e textos longos ocupam toda a coluna do formulário. Preservar condições de habilitação de Salvar/Testar e feedback pelo `NotificationContext`.
3. **Status e ações:** mover o resumo para o painel lateral. Fazer cabeçalho e botões quebrarem/empilharem sem apertar textos. Evitar barra fixa que encubra campos e mensagens.
4. **Aparência:** adicionar prévia simples com os componentes e cores existentes; a seleção continua tendo o mesmo comportamento de persistência. Não introduzir Tailwind na tela administrativa.
5. **Navegação e acessibilidade:** permitir rolagem das abas em largura reduzida; acomodar seletores de transporte em orientação vertical quando necessário; preservar labels, foco, leitura por teclado e ordem do DOM.
6. **Verificação:** validar visualmente e executar `npm run build` e os testes pertinentes. Adicionar testes co-localizados para mudanças funcionais e condições dos botões, sem testar valores CSS como se fossem regras de negócio.

### Critérios de aceite

- Verificar viewports de 1.280, 1.920, 2.560, 3.440 e 5.120 px; verificar também 320, 375, 768 e janela dividida de 1.024 px. Incluir zoom de 125% e 150% em desktop.
- Não haver rolagem horizontal da página, corte de campos, botões sobrepostos ou conteúdo inacessível.
- Formulário e status aproveitarem juntos o painel em desktop; campos manterem largura confortável para leitura.
- Testar temas claro/escuro, SMTP e cada provedor HTTPS, carga, erro, credencial antiga, configuração indisponível e alterações não salvas.
- Preservar Perfil e Segurança em formulários independentes, os estados de envio/teste e a indicação de alterações pendentes.
- Caso continue necessário ampliar Configurações após a comparação visual, avaliar uma exceção local de largura. Alterar o padrão global exige uma decisão separada, pois afeta todas as telas.

## 2. O que ocorre quando o destinatário abre e não clica

1. `EmailMessageComposer` adiciona uma imagem remota de 1 × 1 px com URL assinada de campanha e destinatário.
2. Quando essa imagem é requisitada, `TrackingController.TrackOpen` tenta registrar `Abertura` e devolve um GIF transparente.
3. O registro exige token válido, vínculo do destinatário à campanha e um evento anterior de `Envio`.
4. Requisições repetidas não aumentam a métrica: a ação é única por campanha/destinatário.
5. O dashboard une pixels observados e aberturas inferidas por ações posteriores. “Abriram sem clicar” conta os pares dessa união que não têm clique no recorte analisado.

**Portanto, abrir sem clicar já pode ser contabilizado. O sinal observado é o carregamento de uma imagem remota; não é uma confirmação de leitura humana.**

| Situação | Resultado que o sistema pode observar | Interpretação correta |
| --- | --- | --- |
| Pessoa abre, imagens carregam, não clica | Pixel registrado, sem clique registrado | Sinal de abertura sem clique observado |
| Pessoa abre com imagens bloqueadas | Nenhum pixel e nenhum clique | Interação desconhecida; não concluir que não abriu |
| Serviço de privacidade carrega conteúdo antecipadamente | Pixel registrado mesmo sem leitura | Carregamento automático possível |
| Clique ocorre sem o pixel carregar | Clique e abertura inferida | Interação com o link; leitura não comprovada pelo pixel |
| Link é requisitado por automação | Pode haver clique e abertura inferida | A assinatura valida o link, não identifica um humano |

A Apple documenta que a Proteção de Privacidade do Mail pode baixar conteúdo remoto em segundo plano independentemente da interação. Isso pode elevar “aberturas” sem leitura. [Apple](https://www.apple.com/legal/privacy/data/en/mail-privacy-protection/)

O Outlook clássico bloqueia por padrão o download automático de imagens da Internet. Assim, uma leitura real pode não gerar o pixel. [Microsoft](https://support.microsoft.com/en-us/outlook/block-or-unblock-automatic-picture-downloads-in-classic-outlook-email-messages)

## 3. Achados e prioridades

### Prioridade alta — corrigir antes de usar os indicadores como evidência de resultado

**A. A apresentação promete mais certeza do que a coleta oferece.**

`DashboardKpiGrid`, `RecentCampaignsTable` e `MetricsGlossaryDialog` usam “aberturas efetivas” e “destinatários abriram sem clicar”. Já existe uma distinção observadas/inferidas no tooltip do KPI, o que é positivo, mas falta explicar carregamentos automáticos e imagens bloqueadas.

Proposta: usar “Sinais de abertura”, separar “pixel observado” de “inferido por interação” e chamar o subconjunto de “Sinal de abertura sem clique registrado”. Exibir “Sem interação registrada” para ausência de sinais. Não apresentar isso como taxa de conscientização ou sucesso de treinamento. Atualizar também gráficos e exportações que dependam dessas definições.

**B. A coleta não respeita explicitamente o encerramento da campanha.**

`CampaignStatus` define Finalizada como coleta encerrada e o scheduler finaliza campanhas pela `DataFim`. Porém, `RegistrarAcao` não verifica status nem data. Um link cujo token continue válido pode registrar ações depois do término; o prazo padrão do token é 90 dias.

Proposta: centralizar a política de coleta com relógio injetável. Aceitar eventos durante Processando e Em Andamento, respeitando `DataFim`; rejeitar novos registros após encerramento. A validade do token e a janela da campanha devem ser verificações independentes. Manter o GIF como resposta de abertura rejeitada e definir uma experiência apropriada para links de campanhas encerradas. Testar também a fronteira de horário e a corrida com a finalização.

**C. Há uma janela potencial de perda entre envio externo e persistência do `Envio`.**

`CampaignDispatchService` grava `Envio` somente depois de `sender.SendAsync` retornar sucesso. O tracking exige esse log antes de aceitar abertura/clique. Se um acesso chegar nesse intervalo, será descartado; `TrackOpen` ainda responderá com GIF. A existência dessa janela é inferida da ordem do código; a ocorrência em provedores reais não foi reproduzida nesta revisão.

Proposta: primeiro reproduzir com um sender de teste que provoque a requisição antes de retornar. Se confirmado, manter um recebimento pendente durável, ligado à tentativa existente de `CampaignDelivery`, e promovê-lo a métrica após confirmação do envio. Limitar retenção e deduplicar; reconciliar após restart. A falha de envio não pode virar `Envio` bem-sucedido só para satisfazer o tracking. Não usar espera fixa nem manter transação de banco aberta durante I/O do provedor.

### Prioridade média — robustez e coerência do recorte

**D. Pixel acrescentado após o documento HTML.**

O composer concatena a imagem ao final do texto, inclusive quando o template termina em `</body></html>`. O comportamento depende de como o cliente corrige esse HTML. Inserir o pixel dentro do body para documentos completos e ao final para fragmentos; manter apenas um pixel do sistema. Os testes atuais verificam presença da URL, mas não sua localização em documentos completos. Não foi comprovado que esse detalhe seja a causa de falhas em clientes específicos.

**E. Coorte de envios e janela de interações precisam estar explícitas.**

`DashboardOverviewService` inclui apenas pares com `Envio` dentro do período selecionado. Uma abertura hoje de um e-mail enviado antes do período fica fora. Isso é coerente com uma coorte de envios, mas diferente de “todas as aberturas ocorridas nos últimos 30 dias”. Preservar a coorte como padrão do MVP e explicar o denominador e o recorte. Relatório de eventos por data, se necessário depois, deve ser separado para não misturar populações.

**F. Alcançabilidade pública é requisito independente do envio.**

O fallback de `PublicApiBaseUrl` no desenvolvimento é localhost, enquanto o compose configura um domínio público. Validar o endereço efetivamente usado no ambiente de envio, HTTPS e roteamento de `/api/tracking`. Um teste SMTP/HTTPS bem-sucedido não demonstra que o cliente ou proxy de imagens consegue alcançar o pixel. Não diagnosticar o ambiente atual como quebrado sem testar sua URL efetiva.

**G. Isolamento de tenant requer um ajuste arquitetural.**

O controller verifica token, campanha e associação do alvo ao mesmo tenant; isso já evita injeção trivial por GUID. Entretanto, usa `IgnoreQueryFilters()` durante uma requisição HTTP, contrariando o AGENTS.md, e as buscas de predecessor/duplicata não repetem `TenantId`. Não foi demonstrado vazamento nesta revisão.

Proposta: mover a resolução inicial do link assinado para uma abstração restrita de tracking, sem expor consulta genérica; após obter o tenant validado, executar leituras/escritas num contexto com escopo explícito desse tenant e filtros ativos. Incluir tenant em todas as verificações. Não desabilitar filtros globalmente nem confiar num tenant fornecido pelo chamador. Cobrir tokens e vínculos de tenants distintos.

### Evolução posterior

- Registrar procedência e suspeita de automação apenas se houver benefício mensurável, com retenção limitada. IP/User-Agent não devem ser tratados como prova de leitura humana, e o IP de um proxy não identifica o destinatário.
- Separar aceite de envio pelo transporte de entrega na caixa postal. A confirmação atual de envio não comprova entrega; eventual métrica de entrega requer eventos confiáveis do provedor.
- Qualquer alteração de semântica deve preservar os logs históricos e explicitar limites: dados antigos não permitem recuperar leituras que nunca geraram pixel nem distinguir retroativamente todos os acessos automáticos.

## 4. Sequência recomendada e verificação

1. Ajustar linguagem e glossário; definir a janela de coleta e documentar a coorte de envios.
2. Corrigir encerramento da coleta e escopo de tenant com testes de fronteira, idempotência e isolamento.
3. Reproduzir a corrida envio/tracking; implementar persistência pendente e reconciliação apenas com o cenário caracterizado.
4. Ajustar composição do pixel e validar o endereço público efetivo. Realizar ensaio controlado em clientes reais, com/sem imagens e sem clicar.
5. Implementar o layout de Configurações e validar a matriz visual.

Testes adicionais relevantes:

- Pixel válido após envio, sem clique: uma abertura e zero cliques, inclusive após repetição.
- Pixel inválido/expirado, alvo fora da campanha e cross-tenant: nenhum evento; resposta de imagem não revela se o alvo existe.
- Campanha finalizada e `DataFim` vencida mesmo antes do ciclo do worker: nenhum novo evento.
- Coleta durante Processando: válida para destinatário já enviado; não bloquear o lote inteiro por status.
- Acesso durante o retorno do sender: recebimento não perdido após envio confirmado, não contabilizado como envio confirmado em falha, reconciliação idempotente após restart.
- Documento HTML completo e fragmento: URL assinada correta, pixel do sistema único e dentro do body quando existente.
- Clique sem pixel: inferência separada; ausência de ambos: desconhecido. Nenhum teste sintético deve prometer detectar leitura humana.
- Fronteiras do período, UTC/Brasília, envio anterior ao recorte, mesmo alvo em duas campanhas e totais/exportações coerentes.
- Concorrência em PostgreSQL: requisições simultâneas geram uma ação, sem testar apenas com EF InMemory.

Nenhuma correção proposta deve remover assinatura dos links, associação campanha/alvo, deduplicação ou sanitização do conteúdo, nem passar a armazenar credenciais submetidas nas simulações.

## 5. Análise específica: carregar o pixel no Mailtrap

Atualização após a revisão do teste manual pelo usuário: no Sandbox, apenas abrir a mensagem não gerou abertura; clicar no botão elevou abertura e clique simultaneamente. Isso é compatível com o cálculo atual: clique sem pixel gera abertura inferida. A hipótese inicial de clique automático ao visualizar a mensagem não foi confirmada. Um filtro experimental por metadados de navegação foi retirado antes da entrega; endpoints e testes de tracking foram restaurados ao estado anterior. O próximo ensaio será realizado pelo usuário numa caixa Gmail real, comparando abertura sem clique e clique posterior em uma campanha nova.

### Qual produto está integrado

`ProviderApiEmailSender` usa `https://sandbox.api.mailtrap.io/api/send/{sandboxId}` e envia o HTML montado pelo PhishGuard, inclusive o pixel próprio. Esta é a integração **Mailtrap Sandbox**, destinada a testes, não a integração de envio de produção. A visualização HTML do Sandbox abre por padrão ao inspecionar a mensagem e renderiza seu conteúdo no navegador. [Mailtrap: Email Template Inspector](https://docs.mailtrap.io/email-sandbox/testing/email-template)

### O significado de uma abertura no PhishGuard

Se a visualização do Sandbox requisitar o pixel do PhishGuard e as verificações do endpoint forem satisfeitas, a ação será registrada como `Abertura` para o destinatário simulado da URL. **Esse registro comprova uma requisição à imagem; não que o endereço destinatário recebeu ou leu o e-mail.** O administrador que inspeciona a mensagem pode gerar esse sinal. A mesma atribuição acontece ao abrir o HTML fora do Sandbox ou copiar a URL assinada do pixel: o sistema atribui pelo link, não pela identidade de quem o abriu.

Esta conclusão decorre do código do PhishGuard e do papel documentado da visualização HTML. Não foi observada uma requisição numa sessão real do Mailtrap. A documentação consultada não permite garantir que toda visualização, aba ou recurso de preview sempre carregue nosso pixel: bloqueios, políticas de imagem e cache podem mudar o resultado.

O token assinado previne a fabricação trivial de links por GUID, mas não certifica que quem fez a requisição é o destinatário. IP também não comprova essa identidade. Reabrir a mensagem não deve somar outra abertura: há deduplicação por campanha, alvo e ação.

### As estatísticas do Mailtrap são outra origem

O tracking de abertura documentado para o produto Mailtrap Email API/SMTP de produção usa um pixel adicionado pelo próprio provedor e gera eventos nos seus Email Logs. Não confundir isso com o pixel próprio enviado ao Sandbox. [Mailtrap: Tracking Settings](https://docs.mailtrap.io/email-api-smtp/setup/sending-domain)

Na implementação revisada, a abertura do PhishGuard nasce no `TrackingController`; não há importação de webhooks de abertura do Mailtrap para `SimulationLog`. Portanto, um indicador mostrado pelo provedor não sincroniza automaticamente a abertura no nosso dashboard. Logos anexados por CID são outra coisa: podem aparecer sem que a imagem remota de tracking seja requisitada.

### Por que pode não aparecer “abriu sem clicar”

Verificar nesta ordem, sem clicar no botão da isca:

1. **HTML enviado:** confirmar em HTML Source que existe o `img` de `/api/tracking/open/` com os IDs esperados e token. Conferir também se a imagem foi acrescentada depois de `</html>`; a inserção dentro do body continua recomendada.
2. **Requisição:** nas ferramentas de rede do navegador, conferir se visualizar a aba HTML realmente requisita essa imagem e qual URL foi usada. Usar uma campanha de teste dedicada para não contaminar uma campanha real. Não compartilhar o token nem gravá-lo em relatórios.
3. **Acesso ao endpoint:** verificar HTTPS, roteamento, eventual bloqueio/cache e disponibilidade do endereço. URL de localhost depende de quem faz a requisição e não serve para um proxy remoto. Imagens CID visíveis não validam esse acesso.
4. **Aceitação do evento:** um GIF/HTTP 200 não prova persistência. `TrackOpen` devolve imagem mesmo quando token, associação ou predecessor `Envio` impedem o log. Conferir a existência do registro da campanha/alvo no banco ou numa consulta autorizada do sistema, sempre com tenant.
5. **Dashboard:** confirmar período e departamento. O serviço exige `Envio` no período selecionado; abrir hoje um envio anterior ao período não o inclui nesse recorte. Após a abertura, atualizar o dashboard.

Resultado esperado do ensaio: após o envio confirmado, uma primeira requisição válida do pixel gera **1 abertura e 0 cliques**; repetir mantém os totais. Ao clicar depois, gera **1 clique**, a abertura continua única e o par deixa o subconjunto “sem clique”. Antes de requisitar a imagem não há garantia de abertura registrada. O ensaio valida o mecanismo técnico, não a leitura humana.

### Recomendação para o MVP

- Usar o Sandbox para verificar composição, transporte e funil técnico; identificar seus resultados como simulação/teste.
- Diferenciar visualmente “capturado no Sandbox” de envio externo e substituir “abertura efetiva” por um sinal de abertura com origem explicada.
- Para separar campanhas de teste de métricas reais de modo confiável, persistir no envio o transporte/provedor e o modo de teste. `CampaignDelivery` atual não guarda esses atributos; consultar somente a configuração atual do tenant classificaria incorretamente campanhas antigas depois de uma troca de provedor. Tratar também SMTP Sandbox, sem presumir que apenas API Mailtrap gera dados de teste.
- Priorizar isolamento dos resultados de teste e clareza dos textos antes de filtros heurísticos de automação. Não tentar identificar toda requisição de preview por IP ou User-Agent.
- As correções de janela da campanha, inserção do pixel e corrida de envio continuam pertinentes; não foram implementadas nesta etapa de responsividade.

## 6. Investigação após teste local com Gmail e Mailtrap

O usuário confirmou que testou localmente com SMTP, sem identificar um domínio público. SMTP é o transporte da mensagem; o pixel permanece uma requisição HTTP independente.

### Evidências obtidas

- `appsettings.json` define `PublicApiBaseUrl=http://localhost:5000` e `PublicAppBaseUrl=http://localhost:5173`. O perfil de execução não substitui essas URLs. As duas variáveis públicas estavam ausentes no ambiente deste processo e no `.env` inspecionado (somente presença das chaves verificada, sem expor segredos).
- `run-local.bat` não carregava essas variáveis do `.env`. Esse detalhe foi corrigido: quando informadas, agora são repassadas à API. O script também explica que enviar por SMTP não publica o pixel. `.env.example` ganhou exemplos comentados, sem criar um túnel ou trocar URLs automaticamente.
- O endpoint local respondeu `200 image/gif`, com 42 bytes e sem cache, numa requisição sem token e com GUIDs vazios. Isso só verifica a resposta da imagem; não registra abertura nem prova acesso pela Internet.
- Experimento em Chromium com os seis HTMLs oficiais, reproduzindo o pixel e seus estilos atuais, fez uma requisição à imagem ao renderizar, sem clicar. O navegador reparou a imagem acrescentada após `</html>`. Esse resultado não representa o sanitizador dos serviços de e-mail, mas impede concluir que a posição por si só explique o defeito.
- Foram acrescentados três testes em `PhishGuard.Tests/Controllers/TrackingOpenHttpTests.cs`, todos aprovados: servidor Kestrel em porta efêmera, SQLite em memória com FKs e token HMAC real, usando o composer real. Sem SMTP, acesso a contas ou banco existente.

| Requisição HTTP real no teste isolado | Resultado persistido |
| --- | --- |
| Pixel válido, depois do envio; repetir a requisição | 1 abertura observada, 0 cliques; dashboard com 100% abertura e 0% clique para esse único envio |
| Token adulterado | GIF 200, nenhum evento |
| Pixel antes do registro de envio | GIF 200, nenhum evento; uma nova requisição depois do registro de envio gera abertura |

### Conclusão e limite

O mecanismo registra abertura sem clique quando a imagem chega corretamente à API. No fluxo local padrão, ele cria URLs de localhost. O Gmail usa servidores de proxy para buscar imagens, e a Google documenta que URLs dependentes de endereços internos podem falhar. Esses servidores não acessam o localhost do computador que executa o PhishGuard. [Google: Image URL proxy](https://knowledge.workspace.google.com/admin/gmail/advanced/set-up-an-image-url-proxy-allowlist?hl=en)

Isso explica por que um clique no próprio computador pode chegar à API, enquanto o pixel no Gmail não chega. O dashboard então infere abertura a partir do clique. **A causa no e-mail específico ainda depende de confirmar seu `img src`: a mensagem efetivamente enviada não foi acessada nesta investigação.** O teste do Mailtrap também não foi reproduzido numa sessão real; sua renderização/bloqueio de imagens permanece uma variável não verificada.

Não há evidência suficiente para trocar o HTML ou reescrever a fórmula de abertura como solução desse problema. Também não foi demonstrado que a corrida antes do `Envio` tenha ocorrido nos testes manuais, embora o descarte nessa condição tenha sido reproduzido.

### Como concluir o ensaio real

1. Conferir no HTML original do e-mail o host do `img` que contém `/api/tracking/open/`, sem compartilhar o token. Se for localhost/127.0.0.1/endereço privado, corrigir a URL pública antes de novo teste.
2. Disponibilizar API e frontend em HTTPS público acessível pelo destinatário e pelo serviço de imagens. Isso requer um domínio implantado ou um túnel ativo; nenhuma exposição externa foi iniciada nesta investigação. A URL da imagem deve devolver a imagem diretamente, sem login ou tela intermediária.
3. Informar `AppSettings__PublicApiBaseUrl` e `AppSettings__PublicAppBaseUrl` no ambiente ou `.env` usado pelo `run-local.bat`, reiniciar a API e gerar um novo envio. Alterar configuração não muda as URLs de mensagens antigas; usar uma campanha nova também evita confundir eventos já deduplicados.
4. Abrir com imagens externas habilitadas, sem clicar; atualizar o dashboard e conferir se a ação observada entrou. Só depois clicar e comparar. Mesmo com endpoint público, clientes podem bloquear imagens; a ausência de evento não prova ausência de leitura. [Google: imagens no Gmail](https://support.google.com/mail/answer/145919?co=GENIE.Platform%3DDesktop&hl=pt-BR)

O problema nas caixas reais não foi declarado resolvido. A entrega confirma o funcionamento técnico local, corrige o carregamento de configuração no atalho Windows e identifica o requisito de acesso público que falta validar no ensaio real.
