import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  // tailwindcss() logo após o react(): a esteira do Vite passa a compilar as
  // classes utilitárias do Tailwind v4 automaticamente (substitui o antigo
  // @tailwindcss/postcss, que ficava sem efeito por causa das diretivas v3).
  plugins: [react(), tailwindcss()],
})
