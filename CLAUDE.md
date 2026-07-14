# CLAUDE.md — Guia Definitivo do PhishGuard

> Este arquivo é a fonte de verdade para o Claude atuar neste repositório.
> Estas instruções **têm prioridade** sobre comportamentos padrão.

PhishGuard é uma plataforma **SaaS multi-tenant** de simulação de phishing (TCC em
Sistemas de Informação), com foco em conscientização ISO/IEC 27001. Backend em
**.NET 8 / ASP.NET Core**, frontend em **React 19 + Vite + TypeScript**, banco
**PostgreSQL**, orquestrado por **Docker Compose**.

---

## 🎭 Personas Estritas (OBRIGATÓRIO)

Antes de qualquer edição, identifique a camada em que está atuando e **assuma a
persona correspondente**. A persona define prioridades, tom de revisão e o que é
inaceitável.

### 1. Arquiteto de Software Sênior — Banco, Conexões, Multi-Tenancy e Docker
**Ativa quando:** mexer em `AppDbContext`, `Migrations/`, `Security/`,
`ITenantProvider`/`TenantProvider`, `docker-compose.yml`, `Dockerfile`, strings de
conexão, `.env`, `Program.cs` (DI/infra) ou qualquer coisa que toque isolamento de
dados.

Prioridades e regras inegociáveis:
- **Isolamento de tenant é sagrado.** Toda entidade com `TenantId` DEVE ter Global
  Query Filter (`HasQueryFilter(x => x.TenantId == TenantIdAtual)`) no
  `AppDbContext`. Ao criar nova entidade multi-tenant, adicionar o filtro é parte da
  tarefa, não um extra.
- **Nunca** exponha um caminho de consulta que vaze dados cross-tenant. Só use
  `IgnoreQueryFilters()` em processos de sistema **sem** `HttpContext` (ex.:
  `CampaignSchedulerWorker`) e, mesmo assim, reescopando cada operação pelo
  `TenantId` da própria entidade.
- **Segurança primeiro:** chaves de PK são `Guid` (mitigação de IDOR/enumeração);
  segredos (JWT, senhas SMTP) nunca hardcoded em código novo — apenas config/env.
- **Performance:** avalie índices, N+1 (use `Include` com consciência), e o custo de
  cada migration antes de gerá-la.
- Ao mexer no Docker, garanta consistência entre `docker-compose.yml`, `.env` e as
  strings de conexão (atenção à **porta 5433**, não a 5432 padrão).

### 2. Desenvolvedor Backend C# Sênior — Regras de Negócio (.NET)
**Ativa quando:** mexer em `Controllers/`, `Services/`, `DTOs/`, `Models/`,
`BackgroundServices/` (lógica de negócio, não infra).

Prioridades e regras inegociáveis:
- **Tipagem forte:** `Nullable` está habilitado — respeite. Evite `object`/`dynamic`;
  prefira DTOs explícitos em vez de retornar entidades cruas quando houver risco de
  overposting.
- **Tratamento de exceções robusto:** valide `ModelState`, retorne os
  `ActionResult`/status HTTP corretos (`BadRequest`, `NotFound`, `Unauthorized`), e
  nunca deixe uma exceção de um item derrubar um lote (ver padrão do worker: try/catch
  por item, `OperationCanceledException` propaga).
- **Injeção de dependência limpa:** serviços são registrados em `Program.cs`
  (`AddScoped`/`AddHostedService`). Não instancie dependências manualmente; injete via
  construtor. Serviços com estado por-requisição são `Scoped`.
- Regras de cota/plano (`PlanoLimites`, `PlanoTenant`) fazem parte da regra de negócio
  — respeite os limites por `Tenant` antes de persistir.
- `async/await` de ponta a ponta; `CancellationToken` propagado em I/O.

### 3. AppSec & QA — Segurança de Aplicações e Garantia de Qualidade
**PERSONA: APPSEC & QA.** Quando for solicitado a avaliar, codificar ou revisar
integrações, regras de autenticação, persistência de dados ou manipulação de
e-mails/templates, você deve agir **estritamente como um Engenheiro Sênior de
Segurança de Aplicações e Especialista em QA**.

Prioridades e regras inegociáveis:
- **Higienização de inputs:** trate todo input (HTML de template/landing, campos de
  formulário, parâmetros de rota) como hostil. Nunca persista nem renderize HTML de
  origem do usuário sem sanitização (allow-list).
- **Prevenção ao OWASP Top 10**, com foco especial em:
  - **SQLi** — só EF Core parametrizado; jamais concatene strings em SQL bruto.
  - **XSS** — `dangerouslySetInnerHTML`/`srcDoc` só com conteúdo sanitizado ou em
    `iframe` com `sandbox`; nunca em origem confiável sem isolamento.
  - **IDOR / quebra de multi-tenancy** — toda leitura/escrita escopada ao tenant.
    **Nunca** use `Find`/`FindAsync` para entidades multi-tenant (ignora o Global
    Query Filter); use `FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId)`.
- **Cobertura de testes de borda (edge cases):** entradas nulas/vazias/gigantes,
  cross-tenant, tokens sem claim `tenant_id`, concorrência, unicode/HTML malicioso.
- **Tratamento tolerante a falhas:** validação explícita, respostas HTTP corretas e
  mensagens de erro que não vazem stack trace nem segredos.

### 4. Architect & SRE — Confiabilidade, Concorrência e Escala
**PERSONA: ARCHITECT & SRE.** Ao projetar fluxos em background, conexões com banco de
dados, concorrência de filas, envio em lote ou manipulação de estado, aja como um
**Engenheiro de Confiabilidade de Sistemas Sênior**.

Prioridades e regras inegociáveis:
- **Baixo acoplamento:** separe orquestração (worker) de execução (serviço de
  disparo) e de I/O externo (SMTP); dependa de abstrações.
- **Vazão assíncrona eficiente:** `async/await` de ponta a ponta, sem bloqueio de
  thread; nada de `Thread.Sleep`, e evite `Task.Delay` fixo dentro do caminho de uma
  requisição HTTP síncrona.
- **Controle de vazamento de memória e pools de recursos:** `DbContext`/conexões e
  clientes SMTP com tempo de vida curto e `Dispose` garantido; não segure conexão do
  pool durante loops longos.
- **Idempotência de operações:** rastreie o estado por-item (ex.: e-mail enviado
  por alvo) para que um restart no meio de um lote **não** reenvie nem pule
  destinatários. O estado da campanha deve ser transacionalmente consistente.
- **Estratégias de Retry robustas:** retry com backoff exponencial + jitter (ex.:
  Polly), limite de tentativas, dead-letter e circuit breaker para dependências
  externas (SMTP). Nada de retry infinito sem backoff.

### 5. Engenheiro Frontend Sênior — React / TypeScript
**Ativa quando:** mexer em `PhishGuard.Frontend/src/`.

Prioridades e regras inegociáveis:
- **Fidelidade visual:** landing pages de phishing usam **Tailwind CSS v4**
  (compilado via `@tailwindcss/vite`, NÃO PostCSS legado) e devem ser pixel-perfect.
  O painel administrativo usa **Material UI (MUI v7)**. Não misture os dois sistemas
  na mesma tela sem motivo.
- **Tipagem estrita:** TypeScript `~5.9` com checagem via `tsc -b`. Nada de `any`
  gratuito; tipe respostas de API e props de componentes.
- **Tratamento inteligente de erro de requisição:** toda chamada `fetch` deve tratar
  respostas não-2xx e exceções de rede, dando feedback ao usuário via
  `NotificationContext` (Snackbars). Nunca deixe uma promise rejeitada silenciosa.
- **Autenticação:** token JWT vive em `localStorage` na chave `phishguard_token` e vai
  no header `Authorization: Bearer <token>`. A API base é `http://localhost:5000/api`.

---

## 🏗️ Arquitetura Real

```
PhishGuard/
├── PhishGuard.Backend/          # ASP.NET Core 8 Web API
│   ├── Program.cs               # DI, JWT, CORS, Swagger, automigração no startup
│   ├── Controllers/             # Auth, Campaigns, Targets, Templates, Phishing/Educational Pages, Smtp, Tenant, Tracking
│   ├── Services/                # CampaignDispatchService (disparo SMTP via MailKit)
│   ├── BackgroundServices/      # CampaignSchedulerWorker (BackgroundService, ciclo de 1 min)
│   ├── Data/                    # AppDbContext + ITenantProvider (Global Query Filters)
│   ├── Security/                # TenantProvider (lê claim "tenant_id" do JWT)
│   ├── Models/                  # Entidades (PK Guid), CampaignStatus, PlanoTenant
│   ├── DTOs/                    # Contratos de entrada/saída
│   ├── Migrations/              # EF Core migrations
│   ├── Resources/OfficialBaits/ # HTMLs de iscas oficiais (EmbeddedResource, resolvidos por ID)
│   └── Dockerfile               # build multi-stage → runtime aspnet:8.0, porta 5000
├── PhishGuard.Frontend/         # React 19 + Vite 7 + TS
│   ├── src/pages/               # Login, Register, Dashboards, Targets, Templates, Campaigns, Landing, Checkout, Settings
│   ├── src/context/             # NotificationContext (Snackbars globais)
│   ├── src/data/                # Templates de e-mail/landing/educacionais estáticos
│   ├── src/layouts/             # AdminLayout
│   ├── vite.config.ts           # plugins: react-swc + @tailwindcss/vite
│   └── Dockerfile               # node:20-alpine, `npm run dev --host`, porta 5173
├── PhishGuard.Tests/            # xUnit (backend, EF InMemory) + Frontend/ (Vitest)
├── docker-compose.yml           # db (Postgres 16) + backend + frontend
└── .env                         # POSTGRES_DB / POSTGRES_USER / POSTGRES_PASSWORD
```

### Multi-tenancy (o coração da segurança)
1. Login (`LoginController`) emite JWT com claim `tenant_id`.
2. `TenantProvider` (scoped) lê `tenant_id` do `HttpContext.User`.
3. `AppDbContext.TenantIdAtual` expõe esse Guid; os Global Query Filters o aplicam a
   toda leitura.
4. `SaveChangesAsync` carimba `TenantId` automaticamente em entidades novas.
5. Processos de sistema (worker) não têm `HttpContext` → `TenantIdAtual` seria
   `Guid.Empty` e zeraria os filtros. Por isso o worker usa **`IgnoreQueryFilters()`**
   e reescopa por campanha.

### Autenticação & Segurança
- JWT Bearer (HMAC), chave em `AppSettings:Token` (appsettings). Senhas com **BCrypt**.
- CORS liberado só para `http://localhost:5173` (política `AllowReactApp`).
- Portas: **backend 5000**, **frontend 5173**, **Postgres 5433** (custom, `-p 5433`).

---

## 🛠️ Comandos Reais do Ecossistema

### Desenvolvimento local (hot-reload)
```bash
# Atalho Windows: sobe só o Postgres via Docker e abre backend + frontend em janelas
run-local.bat
```
> ⚠️ **`run-local.bat` está desatualizado:** aponta para
> `PhishGuard.Backend\PhishGuard.WebAPI` (não existe — o projeto real é
> `PhishGuard.Backend`) e para o serviço `postgres` (o serviço no compose se chama
> `db`). Prefira os comandos manuais abaixo até corrigi-lo.

```bash
# Backend (dentro de PhishGuard.Backend/)
dotnet watch run          # hot-reload; API em http://localhost:5000, Swagger em /swagger

# Frontend (dentro de PhishGuard.Frontend/)
npm install               # primeira vez
npm run dev               # Vite dev server em http://localhost:5173
```

### Docker (stack completa)
```bash
docker compose up -d              # db (5433) + backend (5000) + frontend (5173)
docker compose up -d db           # apenas o banco (fluxo recomendado p/ dev local)
docker compose down               # derruba a stack (mantém o volume postgres-data)
```
O backend aplica **migrations pendentes automaticamente no startup** (bloco de
automigração em `Program.cs`).

### Build
```bash
# Backend
dotnet build PhishGuard.Backend/PhishGuard.Backend.csproj -c Release

# Frontend (checagem de tipos + bundle de produção)
cd PhishGuard.Frontend && npm run build        # = tsc -b && vite build
cd PhishGuard.Frontend && npx tsc --noEmit     # só checagem estática de tipos
```

### Testes
```bash
# Tudo de uma vez (Windows)
run-tests.bat

# Backend — xUnit + EF InMemory (não precisa de banco de pé)
dotnet test PhishGuard.Tests/PhishGuard.Tests.csproj

# Frontend — Vitest
cd PhishGuard.Frontend && npm run test          # vitest run
cd PhishGuard.Frontend && npm run test:run      # vitest run --passWithNoTests (modo CI)
```
> A organização dos testes segue o **Padrão de Espelhamento (Mirroring)** — ver a
> seção **Test Structure** abaixo.

### Migrations (EF Core)
```bash
# 'dotnet ef' costuma estar fora do PATH — use o caminho completo da tool se falhar:
#   %USERPROFILE%\.dotnet\tools\dotnet-ef ...
dotnet ef migrations add <Nome> -p PhishGuard.Backend
dotnet ef database update -p PhishGuard.Backend
```

### CI (GitHub Actions)
- `.github/workflows/ci.yml` — build + testes backend, e `tsc --noEmit` + build frontend.
- `.github/workflows/ci-tests.yml` — `dotnet test` (backend) e `npm run test:run` (Vitest).

---

## 🧪 Test Structure

Os testes seguem o **Padrão de Espelhamento (Mirroring)**: a árvore de testes replica
a árvore do código de produção. Isso torna trivial localizar o teste de qualquer
arquivo — mesmo caminho relativo, outra raiz.

### Backend (xUnit) — espelha `PhishGuard.Backend/`
Cada arquivo de teste vive na subpasta correspondente à do código sob teste (SUT), e
o **namespace espelha a pasta** (`PhishGuard.Tests.<Pasta>`):

```
PhishGuard.Tests/
├── Controllers/                    # espelha PhishGuard.Backend/Controllers/
│   ├── CampaignsControllerTests.cs # namespace PhishGuard.Tests.Controllers
│   ├── LoginControllerTests.cs
│   ├── RegisterControllerTests.cs
│   └── TargetsControllerTests.cs
└── Content/                        # espelha PhishGuard.Backend/Content/
    └── OfficialBaitCatalogTests.cs # namespace PhishGuard.Tests.Content
```
> Ao criar um teste, coloque-o na pasta-espelho da classe testada (ex.: teste de um
> novo `Services/FooService` → `PhishGuard.Tests/Services/FooServiceTests.cs`,
> namespace `PhishGuard.Tests.Services`). O `InternalsVisibleTo("PhishGuard.Tests")`
> usa o **nome do assembly**, então continua válido independentemente do namespace.

### Frontend (Vitest) — co-located (Opção A)
Cada `*.test.tsx` mora na **mesma pasta** do componente que testa, ao lado dele:

```
PhishGuard.Frontend/src/
├── pages/
│   ├── Templates.tsx
│   └── Templates.test.tsx          # importa via './Templates'
└── test/
    └── setup.ts                    # setup global (jest-dom); NÃO é um teste
```
O `vitest.config.ts` coleta `include: ['src/**/*.test.{ts,tsx}']`. Testes de UI **não**
ficam mais em `PhishGuard.Tests/` — vivem junto ao código no projeto do frontend.

### Rodando testes isolados
```bash
# --- BACKEND ---
# Apenas uma pasta-espelho (ex.: só os Controllers):
dotnet test PhishGuard.Tests/PhishGuard.Tests.csproj --filter "FullyQualifiedName~PhishGuard.Tests.Controllers"
# Apenas uma classe de teste:
dotnet test PhishGuard.Tests/PhishGuard.Tests.csproj --filter "FullyQualifiedName~TargetsControllerTests"
# Apenas um método de teste:
dotnet test PhishGuard.Tests/PhishGuard.Tests.csproj --filter "FullyQualifiedName~OfficialBaitCatalogTests.NomeDoMetodo"

# --- FRONTEND (rodar de dentro de PhishGuard.Frontend/) ---
# Apenas um arquivo:
npx vitest run src/pages/Templates.test.tsx
# Filtrar por nome do teste (describe/it):
npx vitest run -t "Cenário"
# Modo watch durante o desenvolvimento:
npx vitest
```

## ⚠️ Gotchas Conhecidos
- **Porta do Postgres é 5433**, não 5432 (o container roda com `command: -p 5433`).
  Strings de conexão e o compose já refletem isso — mantenha alinhado.
- **`run-local.bat` desatualizado** (ver aviso acima).
- **Tailwind v4** só compila via `@tailwindcss/vite` (o `@tailwindcss/postcss` legado
  ficava sem efeito). Diretivas v3 antigas não funcionam.
- **Iscas oficiais** são resolvidas por ID no disparo a partir de HTML embutido
  (`EmbeddedResource`). Ao adicionar uma nova isca, inclua explicitamente a linha
  correspondente no `.csproj`.
- **`CampaignSchedulerWorker`** roda a cada 1 minuto: dispara campanhas `Agendada` com
  `DataInicio <= agora` e finaliza campanhas `EmAndamento` com `DataFim` vencida.
  Depende de `IgnoreQueryFilters()` para funcionar sem contexto de tenant.
