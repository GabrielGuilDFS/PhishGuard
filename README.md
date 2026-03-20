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
* **Contêineres:** Docker e Docker Compose (Orquestração do ambiente de desenvolvimento local para o banco de dados e pgAdmin).
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

### 📧 4. Campanhas para Disparo (Próximo Passo) 
- [ ] Criação de Campanhas (Mapeamento: Alvos + e-mail falso + pagina falsa).
- [ ] Integração com serviço SMTP para envio real dos e-mails.
- [ ] Rastreamento de Ações (Abertura de E-mail, Clique no Link, Submissão de Dados Comprometedores).

---

## 🛡️ Escopo e Limitações (TCC)

Para viabilizar o desenvolvimento dentro do cronograma acadêmico, o projeto segue a filosofia de **MVP (Produto Mínimo Viável)**:

1.  **SaaS Lógico:** A arquitetura suporta múltiplas empresas, mas o cadastro de novos "Tenants" é feito via Banco de Dados, sem tela pública de "Assine Agora".
2.  **Billing Simulado:** A gestão de pagamentos e planos é lógica (bloqueio de recursos), sem integração financeira real (cartão de crédito).
3.  **Infraestrutura:** O foco é a validação da arquitetura de software, utilizando serviços de e-mail sandbox (Mailtrap) para evitar bloqueios de SPAM durante os testes.

---

## 🛠️ Instalação e Execução

### Pré-requisitos
* [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
* [Node.js (LTS v18+)](https://nodejs.org/)
* [PostgreSQL](https://www.postgresql.org/)

### 1. Configuração do Backend
```bash
# Clone o repositório
git clone [https://github.com/GabrielGuilDFS/PhishGuard.git](https://github.com/GabrielGuilDFS/PhishGuard.git)

# Acesse a pasta da API
cd PhishGuard.Backend

# Configure a ConnectionString no appsettings.json
# Certifique-se de que o PostgreSQL está rodando

# Execute as migrações (Criação das tabelas e Tenants)
dotnet ef database update

# Inicie a API
dotnet run
```
### 2. Configuração do Frontend
```bash

# Acesse a pasta do Frontend
cd PhishGuard.Frontend

# Instale as dependências
npm install

# Inicie o servidor
npm run dev
```
### 🔮 Trabalhos Futuros (Roadmap)

* IA Generativa: Implementação de IA para clonagem automática de interfaces de login a partir de URLs.

* PhishButton: Plugin para Outlook/Gmail para denúncia de phishing pelos colaboradores.

* Smishing & Quishing: Expansão para vetores de ataque via SMS e QR Code.

### 👤 Autor

Guilherme Gabriel de Freitas Silva

Projeto acadêmico desenvolvido sob orientação, visando a obtenção do grau de Bacharel em Sistemas de Informação.
### 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo LICENSE para mais detalhes.
