// ============================================================================
// Configuracao runtime do frontend — FONTE UNICA da base da API.
//
// Antes, "http://localhost:5000/api" estava hardcoded em ~17 lugares: portar o
// projeto (ou trocar a porta/host da API) exigia editar todos. Agora vem de uma
// unica variavel de ambiente do Vite, com fallback para o dev local.
//
// Como configurar: crie PhishGuard.Frontend/.env (gitignored) a partir do
// .env.example e defina VITE_API_BASE_URL. Vite so expoe variaveis com prefixo
// VITE_ para o client (import.meta.env). Se nao definir, cai no default abaixo.
//
// Cenarios tipicos:
//   - Dev local (npm run dev):     /api (proxy Vite -> localhost:5000)
//   - Docker/ngrok:                /api (proxy Vite -> backend:5000)
//   - Render Static Site:          https://phishguard-backend.onrender.com/api
// ============================================================================
export const API_BASE: string =
  import.meta.env.VITE_API_BASE_URL ?? '/api';

/** Rotas de autenticação canônicas; o Path do refresh cookie é case-sensitive. */
export const AUTH_API_BASE = `${API_BASE}/auth`;
