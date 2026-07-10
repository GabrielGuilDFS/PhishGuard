// O Tailwind v4 passou a ser compilado pelo plugin @tailwindcss/vite (ver
// vite.config.ts), então o @tailwindcss/postcss foi removido daqui para evitar
// processamento duplicado. Mantém-se apenas o autoprefixer para o CSS geral.
export default {
  plugins: {
    autoprefixer: {},
  },
}