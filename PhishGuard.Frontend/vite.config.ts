import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import tailwindcss from '@tailwindcss/vite'

const securityHeaders = {
  'Content-Security-Policy': [
    "default-src 'self'",
    "script-src 'self'",
    "style-src 'self' 'unsafe-inline'",
    "img-src 'self' data: https:",
    "font-src 'self' data:",
    "connect-src 'self' http://localhost:5000 https: ws: wss:",
    "object-src 'none'",
    "base-uri 'self'",
    "form-action 'self'",
    "frame-ancestors 'none'",
  ].join('; '),
  'Referrer-Policy': 'strict-origin-when-cross-origin',
  'X-Content-Type-Options': 'nosniff',
  'X-Frame-Options': 'DENY',
  'Permissions-Policy': 'camera=(), microphone=(), geolocation=(), payment=()',
}

// O React Fast Refresh do Vite injeta um preambulo de modulo inline. Mesmo com
// `unsafe-inline`, uma CSP no servidor dev impede o plugin SWC de confirmar esse
// preambulo e a raiz React fica vazia. A CSP permanece estrita no `vite preview`
// (artefato de producao); apenas `vite dev` a omite e preserva os demais headers.
const developmentSecurityHeaders = {
  ...Object.fromEntries(
    Object.entries(securityHeaders).filter(([name]) => name !== 'Content-Security-Policy'),
  ),
  'Cache-Control': 'no-store',
}

// https://vite.dev/config/
export default defineConfig({
  // tailwindcss() logo após o react(): a esteira do Vite passa a compilar as
  // classes utilitárias do Tailwind v4 automaticamente (substitui o antigo
  // @tailwindcss/postcss, que ficava sem efeito por causa das diretivas v3).
  plugins: [react(), tailwindcss()],
  server: {
    host: true,
    headers: developmentSecurityHeaders,
    // Atrás do túnel, o Host da requisição é o domínio público do ngrok; o Vite
    // bloqueia hosts desconhecidos por padrão. Libera o domínio (via NGROK_DOMAIN)
    // e os sufixos ngrok. Em dev local sem túnel, isso é inócuo.
    allowedHosts: [
      ...(process.env.NGROK_DOMAIN ? [process.env.NGROK_DOMAIN] : []),
      '.ngrok-free.app',
      '.ngrok.app',
    ],
    // REVERSE PROXY: 1 túnel só aponta para o frontend. Toda requisição /api é
    // repassada internamente ao backend na rede Docker (serviço 'backend'), então a
    // landing/página falsa fala com a API pelo MESMO domínio público (same-origin).
    proxy: {
      '/api': {
        target: 'http://backend:5000',
        changeOrigin: true,
      },
    },
  },
  preview: {
    headers: securityHeaders,
  },
})
