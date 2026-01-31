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

### 🎯 Diferenciais da Solução
* **Arquitetura SaaS Real:** Modelagem de dados com isolamento lógico (*Shared Database*) baseada em `TenantId`.
* **Simulação Realista:** Biblioteca de cenários baseada em ameaças reais (Financeiro, Corporativo, E-commerce).
* **Gestão Eficiente:** Importação em massa de alvos via CSV e filtros de busca em tempo real.
* **Feedback Imediato:** Redirecionamento automático para telas educativas ("Teachable Moments") após a detecção de falha humana.

---

## 🚀 Stack Tecnológica

### Backend (API Multi-Tenant)
* **Linguagem:** C#
* **Framework:** ASP.NET Core 8.0 Web API
* **Segurança de Dados:** Entity Framework Core com **Global Query Filters** (para isolamento de dados entre empresas).
* **Banco de Dados:** PostgreSQL.

### Frontend (SPA)
* **Framework:** React + TypeScript (Vite).
* **UI/UX:**
    * **Material UI (MUI):** Painel Administrativo (Temática Dourada/Enterprise).
    * **Tailwind CSS:** Landing Pages de Phishing (Clonagem pixel-perfect).
* **Funcionalidades:** Context API para Notificações, PapaParse para CSV.

### Infraestrutura & Ferramentas
* **SMTP:** Suporte a SendGrid/Mailtrap para disparo de campanhas.
* **CI/CD:** GitHub Actions (Planejado).

---

## ⚙️ Funcionalidades Implementadas

O projeto encontra-se em desenvolvimento ativo. Abaixo, o status dos módulos principais:

### 🏢 1. Core Administrativo
- [x] **Autenticação Segura:** Login com JWT.
- [x] **Gestão de Alvos (Targets):**
    - CRUD completo (Criar, Editar, Excluir).
    - **Importação em Massa:** Upload de arquivos `.csv` (Nome, Email, Setor).
    - **Filtro Inteligente:** Busca em tempo real por nome, e-mail ou setor.
- [ ] **Configurações (Settings):**
    - Painel com Abas (Tabs).
    - Configuração de Servidor SMTP (Host, Porta, Usuários).
    - Gestão de Perfil do Administrador.
- [ ] **Sistema de Notificações:** Feedback visual via Snackbars/Toasts para todas as ações.

### 🎭 2. Biblioteca de Cenários (Em Desenvolvimento)
- [ ] **Galeria Visual:** Grid de cards exibindo templates de ataque.
- [ ] **Preview:** Modal de pré-visualização de como a vítima receberá o e-mail.
- [ ] **Categorização:** Filtros visuais por dificuldade e tipo (Financeiro, RH, etc.).

### 📧 3. Motor de Disparo (Em Desenvolvimento)
- [ ] Criação de Campanhas (Wizard).
- [ ] Integração com serviço SMTP para envio real.
- [ ] Rastreamento de Cliques (Tracking Pixel/Link).

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
