# 🛡️ PhishGuard

> **Plataforma de Conscientização em Segurança da Informação & Simulador de Phishing Ativo.**

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-TypeScript-blue)](https://reactjs.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14+-336791)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-MIT-green)](./LICENSE)

---

## 📖 Sobre o Projeto

**PhishGuard** é um projeto de conclusão de curso (Sistemas de Informação) desenvolvido para atuar como uma ferramenta de educação e prevenção contra ataques de Engenharia Social no setor financeiro.

Diferente de abordagens passivas (apenas palestras ou vídeos), o sistema opera como um **Simulador de Phishing Ativo**. Ele permite que administradores disparem campanhas controladas de e-mails falsos, interceptando a interação dos colaboradores (cliques e submissão de dados) para oferecer feedback educativo imediato no momento da falha.

### 🎯 Objetivos Principais
* **Simulação Realista:** Criação de clones perfeitos de portais bancários e corporativos.
* **Monitoramento Granular:** Rastreamento individualizado via `TrackingToken`.
* **Feedback Imediato:** Redirecionamento automático para telas educativas após a detecção de vulnerabilidade.

---

## 🚀 Stack Tecnológica

A arquitetura foi desenhada para equilibrar a robustez corporativa no backend com a flexibilidade moderna no frontend.

### Backend (API)
* **Linguagem:** C#
* **Framework:** ASP.NET Core 8.0 Web API
* **ORM:** Entity Framework Core
* **Banco de Dados:** PostgreSQL (via Npgsql)

### Frontend (SPA)
* **Framework:** React + TypeScript (Vite)
* **Estilização Híbrida:**
    * 🎨 **Material UI (MUI):** Para o Painel Administrativo (Dashboards, Tabelas).
    * 🖌️ **Tailwind CSS:** Para as Landing Pages de Phishing (Clonagem pixel-perfect).

### Ferramentas de Apoio
* **Mailtrap:** Sandbox SMTP para testes de envio de e-mail.
* **Ngrok:** Tunelamento para testes de responsividade em dispositivos móveis.

---

## ⚙️ Arquitetura e Fluxo

O sistema opera em dois fluxos distintos de navegação:

### 1. Fluxo do Administrador (Gestão)
1.  O Admin acessa o painel seguro (MUI).
2.  Cria uma **Campanha** selecionando um template (ex: "Senha Expirada") e um grupo de alvos.
3.  A API gera um `TrackingToken` (GUID) único para cada alvo.
4.  O sistema dispara os e-mails contendo links para o Frontend: `phishguard.app/s/{token}`.

### 2. Fluxo da Simulação (Alvo)
1.  O colaborador recebe o e-mail e clica no link.
2.  O React renderiza a **Página Falsa** (Tailwind) baseada no cenário.
3.  Ao tentar logar, os dados são interceptados (senha não é salva, apenas o evento).
4.  A API registra o incidente: `DateClicked`, `DataSubmitted`.
5.  O usuário é redirecionado para a **Tela de Alerta Educativo**.

---

## 🛠️ Instalação e Execução

### Pré-requisitos
* [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
* [Node.js (LTS v18+)](https://nodejs.org/)
* [PostgreSQL](https://www.postgresql.org/)

### 1. Configuração do Backend
```bash
# Clone o repositório
git clone [https://github.com/seu-usuario/phishguard.git](https://github.com/seu-usuario/phishguard.git)

# Acesse a pasta da API
cd phishguard/backend

# Configure a ConnectionString no appsettings.json
# "DefaultConnection": "Host=localhost;Port=5432;Database=phishguard;Username=postgres;Password=suasenha"

# Execute as migrações do banco de dados
dotnet ef database update

# Inicie a API
dotnet run

```

### 2. Configuração do Frontend

```bash
# Acesse a pasta do Frontend
cd phishguard/frontend

# Instale as dependências
npm install

# Inicie o servidor de desenvolvimento
npm run dev

```

> **Nota:** Para o envio de e-mails funcionar, configure as credenciais do **Mailtrap** no `appsettings.json` da API.

---

## 📸 Funcionalidades Detalhadas

| Módulo | Descrição |
| --- | --- |
| **Visão Geral** | Visão geral de campanhas, métricas de abertura e taxa de cliques. |
| **Gestão de Alvos** | Pagina para adicionar vitimas dos testes |
| **Biblioteca** | Templates pré-definidos de e-mails e páginas falsas. |
| **Nova Campanha** | Wizard para criação de disparos em massa. |
| **Configurações** | Página dedicada a mudança de senha e configuração do SMTP. |
---

## 👤 Autor

**Guilherme Gabriel de Freitas Silva**

Projeto desenvolvido como Trabalho de Conclusão de Curso (TCC) em Sistemas de Informação.

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](https://www.google.com/search?q=./LICENSE) para mais detalhes.
