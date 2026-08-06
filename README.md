# 🛡️ PhishGuard SaaS

> **Plataforma de Simulação de Engenharia Social & Compliance ISO 27001.**

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-TypeScript-blue)](https://reactjs.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14+-336791)](https://www.postgresql.org/)
[![Architecture](https://img.shields.io/badge/Architecture-SaaS%20Multi--Tenant-orange)](https://en.wikipedia.org/wiki/Multitenancy)
[![License](https://img.shields.io/badge/License-MIT-green)](./LICENSE)

---

## 📖 Sobre o Projeto

**PhishGuard** é uma plataforma **SaaS (Software as a Service)** desenvolvida como Trabalho de Conclusão de Curso (TCC) em Sistemas de Informação. O objetivo é mitigar o risco humano em segurança da informação através de simulações controladas de ataques de Engenharia Social (Phishing).

O projeto se diferencia por adotar uma **Arquitetura Multi-Tenant** escalável, permitindo que múltiplas organizações utilizem a plataforma de forma isolada e segura. Além disso, a ferramenta foi projetada com foco nos controles de conscientização da norma **ABNT NBR ISO/IEC 27001:2022**, automatizando a educação corporativa e promovendo a cultura de *Privacy by Design*.

### 🎯 Diferenciais da Solução e Arquitetura
* **Segurança e Prevenção de IDOR:** Modelagem de banco de dados utilizando UUIDs (Identificadores Únicos Universais) sequenciais como chaves primárias, garantindo segurança contra ataques de enumeração sem comprometer a performance de indexação (B-Tree) no banco relacional.
* **Arquitetura SaaS Real (Isolamento de Dados):** Abordagem *Shared Database, Shared Schema*. O vazamento horizontal de dados (*cross-tenant data leakage*) é bloqueado na raiz da aplicação utilizando a chave discriminadora `TenantId` em conjunto com *Global Query Filters* no ORM.
* **Gestão Eficiente:** Importação e exportação em massa de alvos (funcionários) otimizada via manipulação de arquivos CSV no *client-side*.
* **Simulação e Feedback:** Biblioteca de cenários baseada em ameaças reais com redirecionamento automático para telas educativas (*Teachable Moments*) após a interação com a campanha maliciosa.
* **Qualidade e Validação (UAT):** Sistema validado através de métricas quantitativas estritas, incluindo Taxa de Conformidade de Requisitos (TCR), *Pass Rate* para fluxos críticos e matriz de densidade de defeitos.

---

## 🚀 Stack Tecnológica

### Backend (API Multi-Tenant)
* **Linguagem:** C#
* **Framework:** ASP.NET Core 8.0 Web API
* **Acesso a Dados:** Entity Framework Core (configurado com *Global Query Filters* e geradores de UUIDs otimizados).
* **Banco de Dados:** PostgreSQL

### Frontend (SPA)
* **Framework:** React + TypeScript (Vite)
* **UI/UX:**
  * **Material UI (MUI):** Painel Administrativo e *Dashboard* Corporativo.
  * **Tailwind CSS:** Construção das *Landing Pages* de Phishing (Clonagem *pixel-perfect*).
* **Funcionalidades:** Context API para gerenciamento de estado e notificações, PapaParse para processamento rápido de CSV.

### Infraestrutura & Ferramentas
* **Contêineres:** Docker e Docker Compose (orquestração do ambiente local: PostgreSQL, backend e frontend).
* **Motor de E-mail:** Protocolo SMTP (Suporte a SendGrid/Mailtrap) para o disparo automatizado das campanhas.
* **CI/CD:** GitHub Actions (Planejado).

---

## ⚙️ Funcionalidades Implementadas

O projeto encontra-se em desenvolvimento ativo. Abaixo, o status dos módulos principais:

### 🏢 1. Core Administrativo
- [x] **Autenticação Segura:** Login com JWT e proteção de rotas privadas.
- [x] **Isolamento de Dados:** Arquitetura Multi-Tenant com *Global Query Filters* garantindo que clientes não vejam dados uns dos outros.
- [x] **Gestão de Alvos (Targets):**
    - CRUD completo (Criar, Editar, Excluir).
    - **Importação em Massa:** Upload ágil via arquivos `.csv` (Nome, Email, Setor).
    - **Filtro Inteligente:** Busca em tempo real na tabela de alvos.
- [x] **Sistema de Notificações:** Feedback visual imersivo via Snackbars (Alertas de sucesso, erro e informação) em todas as ações.
- [ ] **Configurações (Settings):**
    - Painel com Abas (Tabs).
    - Configuração de Servidor SMTP (Host, Porta, Usuários).
    - Gestão de Perfil do Administrador.

### 🎭 2. Biblioteca de Cenários (Iscas de E-mail) - Inicial
- [x] **Gestão de Templates:** CRUD de cenários de phishing com armazenamento seguro de código HTML.
- [x] **Assistente de Criação Rápida:** Construtor dinâmico que cruza Marcas (ex: Microsoft, Google, Intranet) com Gatilhos Mentais (ex: Senha Expirada, RH) para gerar iscas automaticamente.
- [x] **Live Preview Seguro:** Renderização em tempo real do código HTML no painel para auditoria visual antes do disparo, utilizando `dangerouslySetInnerHTML` de forma controlada.

### 🕸️ 3. Páginas Simuladas (Armadilhas / Landing Pages) - Inicial
- [x] **Gestão de Páginas Falsas:** Sistema para armazenar a interface onde a vítima cairá após o clique.
- [x] **Moldes Corporativos:** Templates pré-fabricados de alto risco (ex: Login Microsoft 365, Portal de Intranet Genérico).
- [x] **Isolamento de Visualização:** Live Preview do ataque rodando em ambiente seguro e contido utilizando `iframe` para evitar vazamento de CSS.

### 📧 4. Campanhas para Disparo (Implementado)
- [x] **Criação de Campanhas:** mapeamento de Alvos + e-mail falso + página falsa + página educativa.
- [x] **Agendamento e Disparo Automático:** `CampaignSchedulerWorker` (BackgroundService) dispara no horário agendado e finaliza a coleta automaticamente; disparo assíncrono e **idempotente** (retomada sem duplicar e-mails).
- [x] **Integração SMTP real:** envio via MailKit com credenciais por-tenant (senha cifrada em repouso via Data Protection).
- [x] **Rastreamento de Ações:** Envio, Abertura (pixel), Clique (redirect) e Submissão de Dados registrados em `simulations_logs`.

---

## 🛡️ Escopo e Limitações (TCC)

Para viabilizar o desenvolvimento dentro do cronograma acadêmico, o projeto segue a filosofia de **MVP (Produto Mínimo Viável)**:

1.  **SaaS Lógico:** A arquitetura suporta múltiplas empresas, mas o cadastro de novos "Tenants" é feito via Banco de Dados, sem tela pública de "Assine Agora".
2.  **Billing Simulado:** A gestão de pagamentos e planos é lógica (bloqueio de recursos), sem integração financeira real (cartão de crédito).
3.  **Infraestrutura:** O foco é a validação da arquitetura de software, utilizando serviços de e-mail sandbox (Mailtrap) para evitar bloqueios de SPAM durante os testes.

---

## 🛠️ Instalação e Execução

### Pré-requisitos
* [.NET SDK 8.0+](https://dotnet.microsoft.com/download) — a versão fica **fixada** em `global.json` (`rollForward: latestMinor`), então mesmo com SDK 9/10 instalado os comandos `dotnet` usam a banda 8.x do projeto.
* [Node.js (LTS v20+)](https://nodejs.org/) — versão fixada em `.nvmrc` (bate com a imagem `node:20-alpine` do Docker). Rode `nvm use` para alinhar.
* [Docker + Docker Compose](https://www.docker.com/) (sobe o PostgreSQL)

### 🔧 Configuração (obrigatório na 1ª vez — vale para QUALQUER PC)

Toda a configuração de máquina vive em variáveis de ambiente. O repositório versiona
apenas o **template** `.env.example`; o `.env` real é gitignored (cada dev tem o seu):

```bash
cp .env.example .env      # Windows PowerShell:  Copy-Item .env.example .env
# edite o .env e preencha: POSTGRES_PASSWORD e AppSettings__Token (chave longa/aleatória)
```

> **Porta do Postgres:** controlada por uma única variável `POSTGRES_PORT` no `.env`
> (default **5433**, não 5432, para não conflitar com um Postgres já instalado na sua
> máquina). Mude só ali e o compose inteiro acompanha. **Convenção:** use SEMPRE o
> Postgres do Docker — nunca o nativo do host — assim a porta não diverge entre PCs.
>
> **Segredo do JWT:** `AppSettings__Token` no `.env` (Docker) ou, em dev local sem
> Docker, via user-secrets:
> `dotnet user-secrets set "AppSettings:Token" "<chave-longa>" --project PhishGuard.Backend`.
>
> **Base da API (frontend):** default same-origin `/api`, encaminhado pelo proxy do
> Vite para `http://localhost:5000` no desenvolvimento local. Para alterar o destino
> interno, ajuste `VITE_API_PROXY_TARGET` no `.env` do frontend.

### Opção A — Stack completa via Docker (recomendado)
```bash
git clone https://github.com/GabrielGuilDFS/PhishGuard.git
cd PhishGuard
cp .env.example .env      # e preencha os segredos (ver acima)

# Sobe db + backend (5000) + frontend (5173). A porta do db vem de POSTGRES_PORT (.env).
# O backend aplica as migrations automaticamente no startup — não é preciso rodar
# `dotnet ef database update` manualmente.
docker compose up -d --build
```

### Opção B — Desenvolvimento local com hot-reload
```bash
# Atalho (Windows): sobe só o Postgres (serviço `db`) e abre backend + frontend.
run-local.bat
# Linux/macOS: equivalente multiplataforma
./run-local.sh

# ...ou manualmente, em terminais separados:
docker compose up -d db
cd PhishGuard.Backend && dotnet watch run     # API em http://localhost:5000 (Swagger em /swagger)
cd PhishGuard.Frontend && npm install && npm run dev   # SPA em http://localhost:5173
```

## ☁️ Deploy na Render

O arquivo `render.yaml` na raiz provisiona a arquitetura completa:

- `phishguard-backend`: Web Service Docker (.NET 8), health check em `/health/ready`;
- `phishguard-frontend`: Static Site Vite, com fallback de SPA para `index.html`;
- `phishguard-db`: PostgreSQL conectado ao backend pela rede privada da Render.

O backend usa o plano `starter` porque mantém as chaves de Data Protection em disco
persistente. Essas chaves protegem as credenciais SMTP salvas; usar uma instância sem
disco faria as credenciais existentes ficarem indecifráveis após um novo deploy.

### Primeiro deploy

1. Faça push da branch `main` para o GitHub.
2. Na Render, crie um **Blueprint** a partir deste repositório e mantenha o caminho
   padrão `render.yaml`.
3. Informe `AppSettings__Token` quando solicitado. Use um segredo aleatório com pelo
   menos 64 caracteres; nunca reutilize senha de usuário ou do banco.
4. Aguarde backend, banco e frontend ficarem disponíveis.
5. Valide `/health/live`, `/health/ready`, `/swagger` e o acesso direto a
   `/admin/dashboard` no frontend.

Para um serviço Docker configurado manualmente, use `PhishGuard.Backend/Dockerfile`
como **Dockerfile Path** e deixe o **Docker Build Context** na raiz (`.`). O Dockerfile
usa caminhos relativos à raiz do monorepo.

> Se já existir um serviço Node/Express chamado `phishguard-backend`, exclua ou
> renomeie esse serviço antes de aplicar o Blueprint. A Render não permite trocar o
> runtime de um serviço existente de Node para Docker. A resposta
> `PhishGuard backend is now online!` com header `X-Powered-By: Express` pertence ao
> deploy antigo, não a esta API ASP.NET.

Os hostnames públicos usados no Blueprint são
`phishguard-backend.onrender.com` e `phishguard-frontend.onrender.com`. Caso a Render
atribua nomes diferentes, atualize em conjunto:

- `AppSettings__PublicApiBaseUrl`;
- `AppSettings__PublicAppBaseUrl`;
- `AppSettings__AllowedCorsOrigins__0`;
- `VITE_API_BASE_URL`;
- a origem permitida em `connect-src` no header CSP do Static Site.

Depois de alterar `VITE_API_BASE_URL`, faça novo deploy do frontend: variáveis `VITE_`
são incorporadas ao bundle durante o build.

### 🔮 Trabalhos Futuros (Roadmap)

* IA Generativa: Implementação de IA para clonagem automática de interfaces de login a partir de URLs.

* PhishButton: Plugin para Outlook/Gmail para denúncia de phishing pelos colaboradores.

* Smishing & Quishing: Expansão para vetores de ataque via SMS e QR Code.

### 👤 Autor

Guilherme Gabriel de Freitas Silva

Projeto acadêmico desenvolvido sob orientação, visando a obtenção do grau de Bacharel em Sistemas de Informação.
### 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo LICENSE para mais detalhes.
