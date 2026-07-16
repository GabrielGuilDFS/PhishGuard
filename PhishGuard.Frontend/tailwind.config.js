/** @type {import('tailwindcss').Config} */
// ⚠️ Tailwind v4: a compilação real acontece via @tailwindcss/vite lendo o
// `@theme inline` de src/index.css — este arquivo NÃO entra no build (não há
// `@config`). O mapa abaixo é apenas o registro legível do mesmo contrato:
// utilitário semântico → variável CSS dinâmica. A fonte de verdade dos utilitários
// é src/index.css; a das cores do MUI é src/theme/index.ts. Mapa completo dos
// papéis (MUI ↔ Tailwind ↔ CSS var) em src/theme/themeHelper.ts.
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'text': 'var(--text)',
        'background': 'var(--background)',
        'primary': 'var(--primary)',
        'secondary': 'var(--secondary)',
        'accent': 'var(--accent)',
      },
    },
  },
  plugins: [],
}
