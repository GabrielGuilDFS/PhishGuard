import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
    rules: {
      // Permite exportar componente + constante no mesmo arquivo (helpers puros co-locados).
      'react-refresh/only-export-components': ['error', { allowConstantExport: true }],
      // O codebase embute HTML de e-mail/landing em template literals; whitespace
      // "irregular" (ex.: NBSP) ali dentro e' intencional e nao deve quebrar o lint.
      'no-irregular-whitespace': ['error', { skipStrings: true, skipTemplates: true, skipComments: true }],
    },
  },
  // Context files exportam Provider (componente) + hook de consumo juntos — padrao
  // idiomatico de React Context. A regra de Fast Refresh nao se aplica a eles.
  {
    files: ['src/context/**/*.tsx'],
    rules: {
      'react-refresh/only-export-components': 'off',
    },
  },
])
