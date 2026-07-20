import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  // tailwindcss() logo após o react(): a esteira do Vite passa a compilar as
  // classes utilitárias do Tailwind v4 automaticamente (substitui o antigo
  // @tailwindcss/postcss, que ficava sem efeito por causa das diretivas v3).
  plugins: [react(), tailwindcss()],
  server: {
    host: true,
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
})
