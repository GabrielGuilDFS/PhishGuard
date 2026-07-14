import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react-swc'

// Configuração dedicada da suíte de testes (Vitest + Testing Library).
// Mantida separada do vite.config.ts para não acoplar o build à infra de testes.
//
// Os testes de UI seguem o padrão de espelhamento (mirroring) co-located: cada
// arquivo *.test.tsx mora na MESMA pasta do componente de produção que testa
// (ex.: src/pages/Templates.tsx ↔ src/pages/Templates.test.tsx). Assim a toolchain
// e as suítes vivem juntas na raiz do frontend, sem precisar resolver imports/
// node_modules fora da árvore do projeto.
export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    include: ['src/**/*.test.{ts,tsx}'],
  },
})
