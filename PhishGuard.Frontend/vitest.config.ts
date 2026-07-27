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
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov'],
      // all:true conta TODO o código de produção (inclusive arquivos ainda sem teste)
      // como 0% — medida honesta que expõe os blind spots, não só "do que testamos".
      all: true,
      include: ['src/**/*.{ts,tsx}'],
      exclude: [
        'src/**/*.test.{ts,tsx}',
        'src/test/**',
        'src/main.tsx',
        'src/vite-env.d.ts',
        'src/**/*.d.ts',
      ],
      // Gate (ratchet): baseline real hoje e' ~62% global, 98% src/data, 100% src/auth.
      // Os limites nunca podem CAIR; suba-os conforme os blind spots (LandingPage,
      // dashboards) ganharem cobertura no roadmap de QA.
      thresholds: {
        lines: 60,
        'src/data/**': { lines: 80 },
        'src/auth/**': { lines: 80 },
      },
    },
  },
})
