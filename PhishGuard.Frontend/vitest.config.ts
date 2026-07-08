import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react-swc'
import { createRequire } from 'node:module'

// Configuração dedicada da suíte de testes (Vitest + Testing Library).
// Mantida separada do vite.config.ts para não acoplar o build à infra de testes.
//
// A toolchain de testes (node_modules, React, MUI, Testing Library) vive aqui, em
// PhishGuard.Frontend, então o Vitest roda a partir deste projeto. As suítes de UI,
// porém, ficam separadas do código de produção em PhishGuard.Tests/Frontend.
//
// Como esses testes moram fora da raiz do frontend, seus imports "bare" (vitest,
// @testing-library/react, react/jsx-dev-runtime injetado pelo transform de JSX) não
// encontram o node_modules subindo a árvore. O plugin abaixo resolve esses imports
// a partir do node_modules do frontend, evitando symlinks/junctions.
const requireFromFrontend = createRequire(import.meta.url)

function resolveExternalTestDeps() {
  return {
    name: 'resolve-external-test-deps',
    enforce: 'pre' as const,
    resolveId(source: string, importer: string | undefined) {
      const isBare = !!source && !source.startsWith('.') && !source.startsWith('/') && !source.startsWith('\0')
      const fromExternalTest = !!importer && importer.includes('PhishGuard.Tests')
      // 'vitest'/'@vitest' precisam da resolução ESM interna do próprio Vitest —
      // require.resolve devolveria o entry CommonJS, que o Vitest rejeita.
      const isVitestOwn = source.startsWith('vitest') || source.startsWith('@vitest')
      if (isBare && fromExternalTest && !isVitestOwn) {
        try {
          return requireFromFrontend.resolve(source)
        } catch {
          return null
        }
      }
      return null
    },
  }
}

export default defineConfig({
  plugins: [resolveExternalTestDeps(), react()],
  server: {
    fs: {
      // Permite ao Vitest ler os testes de UI que moram fora da raiz do frontend
      // (em ../PhishGuard.Tests). '..' resolve para a raiz do repositório.
      allow: ['..'],
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    include: [
      'src/**/*.test.{ts,tsx}',
      '../PhishGuard.Tests/**/*.test.{ts,tsx}',
    ],
  },
})
