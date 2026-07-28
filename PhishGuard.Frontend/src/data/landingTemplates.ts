import { type TemplateModel } from './templateTypes';

// Moldes estáticos de LANDING PAGES (páginas de captura) do PhishGuard.
//
// Consolidação da interface "bho MAX - Redefinição de Senha": os subcomponentes
// React originais do v0 (SiteHeader, SiteFooter, ChangePasswordForm, MUI theme e
// globals.css) foram unificados em UMA única string de HTML com CSS embutido.
//
// Telemetria: a tag <form> intercepta o submit via handler inline `onsubmit`
// (handlers inline funcionam mesmo quando o HTML é injetado por innerHTML, ao
// contrário de <script>). Ele dispara o gatilho do PhishGuard
// (POST /api/tracking/submit/{{CAMPAIGN_ID}}/{{TARGET_ID}}) enviando apenas
// propriedades de validação — NUNCA a senha em texto (LGPD) — e então redireciona
// o fluxo para a rota educacional interna (/educational-feedback?template=basico_phishing),
// que renderiza o molde educacional de conscientização. Os placeholders
// {{CAMPAIGN_ID}} e {{TARGET_ID}} são substituídos pelo LandingPage.tsx no momento
// em que o alvo abre o link (usados na telemetria do submit).

const hboMaxRedefinicaoSenhaHtml = `<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Mude sua senha | bho MAX</title>
<style>
  @import url('https://fonts.googleapis.com/css2?family=Figtree:wght@400;600;700;800&display=swap');
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    background: radial-gradient(120% 90% at 50% -10%, #1c1c1f 0%, #111113 35%, #0a0a0b 70%, #050505 100%);
    color: #ffffff;
    font-family: 'Figtree', system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif;
    -webkit-font-smoothing: antialiased;
  }
  a { text-decoration: none; color: inherit; }
  /* ---------- Header (SiteHeader) ---------- */
  .hbo-header { display: flex; align-items: center; justify-content: space-between; padding: 20px 24px; }
  /* Novo logo "bho MAX" (asset SVG em public/, variante p/ fundo escuro). */
  .hbo-logo { display: block; height: 44px; width: auto; }
  .hbo-nav { display: none; gap: 32px; }
  .hbo-nav a { font-size: 15px; font-weight: 700; color: rgba(255,255,255,0.9); }
  .hbo-nav a:hover { color: #ffffff; }
  .hbo-nav a.muted { color: #a3a3a3; }
  .hbo-actions { display: flex; align-items: center; gap: 20px; }
  .hbo-actions svg { width: 24px; height: 24px; color: rgba(255,255,255,0.9); }
  .hbo-avatar { width: 32px; height: 32px; border-radius: 9999px; overflow: hidden; box-shadow: 0 0 0 2px rgba(255,255,255,0.2); background: linear-gradient(135deg,#4b4b4b,#222); }
  @media (min-width: 768px) { .hbo-nav { display: flex; } }
  /* ---------- Main / título ---------- */
  .hbo-main { flex: 1; display: flex; flex-direction: column; align-items: center; padding: 32px 24px 0; }
  .hbo-title { text-align: center; font-size: 2.6rem; font-weight: 700; line-height: 1.1; }
  @media (min-width: 640px) { .hbo-title { font-size: 3rem; } }
  .hbo-subtitle { margin-top: 12px; text-align: center; font-size: 15px; color: #d4d4d4; }
  .hbo-subtitle span { color: #a3a3a3; }
  /* ---------- Card / formulário (ChangePasswordForm) ---------- */
  .hbo-card-wrap { margin-top: 32px; width: 100%; max-width: 42rem; }
  .hbo-card { width: 100%; border-radius: 16px; background: rgba(255,255,255,0.03); box-shadow: inset 0 0 0 1px rgba(255,255,255,0.05); padding: 36px 32px; }
  .hbo-form { display: flex; flex-direction: column; gap: 24px; }
  .hbo-field { display: flex; flex-direction: column; gap: 8px; }
  .hbo-label { font-size: 18px; font-weight: 700; color: #ffffff; }
  .hbo-label .opt { font-weight: 400; }
  /* Input nativo estilizado emulando o MuiOutlinedInput dark do original */
  .hbo-input-wrap { position: relative; display: flex; align-items: center; }
  .hbo-input {
    width: 100%; background: #0d0d0d; color: #ffffff; font-size: 16px; font-family: inherit;
    padding: 16px 48px 16px 14px; border-radius: 8px;
    border: 1px solid rgba(255,255,255,0.25); outline: none; transition: border-color .15s ease;
  }
  .hbo-input:hover { border-color: rgba(255,255,255,0.45); }
  .hbo-input:focus { border-color: #ffffff; border-width: 1.5px; padding: 15.5px 47.5px 15.5px 13.5px; }
  .hbo-eye { position: absolute; right: 8px; background: none; border: none; cursor: pointer; color: rgba(255,255,255,0.85); padding: 8px; display: flex; }
  .hbo-eye svg { width: 20px; height: 20px; }
  .hbo-help { font-size: 15px; line-height: 1.5; color: #a3a3a3; }
  .hbo-actions-row { margin-top: 8px; display: flex; flex-wrap: wrap; gap: 16px; }
  .hbo-btn { font-size: 16px; font-weight: 700; border-radius: 8px; padding: 12px 34px; cursor: pointer; font-family: inherit; border: 1px solid transparent; }
  .hbo-btn-primary { background: #ffffff; color: #000000; }
  .hbo-btn-primary:hover { background: #e6e6e6; }
  .hbo-btn-outline { background: transparent; color: #ffffff; border-color: rgba(255,255,255,0.55); }
  .hbo-btn-outline:hover { border-color: #ffffff; background: rgba(255,255,255,0.06); }
  /* ---------- Footer (SiteFooter) ---------- */
  .hbo-footer { margin-top: auto; padding: 64px 24px 32px; }
  .hbo-social { display: flex; align-items: center; gap: 28px; margin-bottom: 32px; color: rgba(255,255,255,0.9); }
  .hbo-social svg { width: 20px; height: 20px; }
  .hbo-flinks { display: flex; flex-wrap: wrap; gap: 12px 32px; margin-bottom: 16px; }
  .hbo-flinks a { font-size: 14px; font-weight: 600; color: rgba(255,255,255,0.9); }
  .hbo-copy { font-size: 14px; color: #737373; }
  /* ---------- Botão flutuante de chat ---------- */
  .hbo-chat { position: fixed; bottom: 24px; right: 24px; width: 56px; height: 56px; display: flex; align-items: center; justify-content: center; border-radius: 9999px; background: #262626; color: #ffffff; border: 1px solid rgba(255,255,255,0.1); box-shadow: 0 10px 25px rgba(0,0,0,.4); cursor: pointer; }
  .hbo-chat svg { width: 24px; height: 24px; }
</style>
</head>
<body>
  <header class="hbo-header">
    <img class="hbo-logo" src="/bho-max-logo-ondark.svg" alt="bho MAX">
    <nav class="hbo-nav">
      <a href="#">Início</a>
      <a href="#">Séries</a>
      <a href="#">Filmes</a>
      <a href="#" class="muted">bho MAX</a>
      <a href="#">Esportes</a>
      <a href="#">Crianças &amp; Família</a>
    </nav>
    <div class="hbo-actions">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="11" cy="11" r="8"></circle><path d="m21 21-4.3-4.3"></path></svg>
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m19 21-7-4-7 4V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v16z"></path></svg>
      <div class="hbo-avatar"></div>
    </div>
  </header>

  <main class="hbo-main">
    <h1 class="hbo-title">Mude sua senha</h1>
    <p class="hbo-subtitle"><span>*</span> Indica um campo obrigatório.</p>

    <div class="hbo-card-wrap">
      <div class="hbo-card">
        <form class="hbo-form" onsubmit="event.preventDefault();var np=document.getElementById('new-password').value;var cp=document.getElementById('current-password').value;var meta={camposPreenchidos:(cp.length>0&&np.length>0),senhasCoincidem:false,tamanhoSenha:np.length};fetch('/api/tracking/submit/{{CAMPAIGN_ID}}/{{TARGET_ID}}',{method:'POST',headers:{'Content-Type':'application/json','ngrok-skip-browser-warning':'true'},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href='/educational-feedback?template=bhomax&c={{CAMPAIGN_ID}}&t={{TARGET_ID}}';});return false;">
          <div class="hbo-field">
            <label class="hbo-label" for="current-password">Senha atual <span class="opt">*</span></label>
            <div class="hbo-input-wrap">
              <input class="hbo-input" id="current-password" name="current-password" type="password" autocomplete="current-password" required>
              <button type="button" class="hbo-eye" aria-label="Exibir senha" onclick="var i=document.getElementById('current-password');i.type=(i.type==='password')?'text':'password';">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"></path><circle cx="12" cy="12" r="3"></circle></svg>
              </button>
            </div>
          </div>

          <div class="hbo-field">
            <label class="hbo-label" for="new-password">Nova senha <span class="opt">*</span></label>
            <div class="hbo-input-wrap">
              <input class="hbo-input" id="new-password" name="new-password" type="password" autocomplete="new-password" required>
              <button type="button" class="hbo-eye" aria-label="Exibir senha" onclick="var i=document.getElementById('new-password');i.type=(i.type==='password')?'text':'password';">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"></path><circle cx="12" cy="12" r="3"></circle></svg>
              </button>
            </div>
            <p class="hbo-help">A senha precisa ter pelo menos 10 caracteres, com no máximo 4 caracteres repetidos em sequência.</p>
          </div>

          <div class="hbo-actions-row">
            <button type="submit" class="hbo-btn hbo-btn-primary">Salvar</button>
            <button type="button" class="hbo-btn hbo-btn-outline" onclick="window.location.href='/educational-feedback?template=bhomax&c={{CAMPAIGN_ID}}&t={{TARGET_ID}}';">Cancele</button>
          </div>
        </form>
      </div>
    </div>
  </main>

  <footer class="hbo-footer">
    <div class="hbo-social">
      <a href="#" aria-label="YouTube"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M23.5 6.2a3.02 3.02 0 0 0-2.12-2.14C19.5 3.55 12 3.55 12 3.55s-7.5 0-9.38.51A3.02 3.02 0 0 0 .5 6.2C0 8.09 0 12 0 12s0 3.91.5 5.8a3.02 3.02 0 0 0 2.12 2.14c1.88.51 9.38.51 9.38.51s7.5 0 9.38-.51a3.02 3.02 0 0 0 2.12-2.14C24 15.91 24 12 24 12s0-3.91-.5-5.8ZM9.6 15.6V8.4l6.2 3.6-6.2 3.6Z"></path></svg></a>
      <a href="#" aria-label="X"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M18.244 2.25h3.308l-7.227 8.26 8.502 11.24h-6.66l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231 5.45-6.231Zm-1.161 17.52h1.833L7.084 4.126H5.117L17.083 19.77Z"></path></svg></a>
      <a href="#" aria-label="Facebook"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M24 12c0-6.63-5.37-12-12-12S0 5.37 0 12c0 5.99 4.39 10.95 10.13 11.85v-8.38H7.08V12h3.05V9.36c0-3.01 1.79-4.67 4.53-4.67 1.31 0 2.69.23 2.69.23v2.96h-1.51c-1.49 0-1.96.93-1.96 1.87V12h3.33l-.53 3.47h-2.8v8.38C19.61 22.95 24 17.99 24 12Z"></path></svg></a>
      <a href="#" aria-label="Instagram"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2.16c3.2 0 3.58.01 4.85.07 1.17.05 1.8.25 2.23.41.56.22.96.48 1.38.9.42.42.68.82.9 1.38.16.42.36 1.06.41 2.23.06 1.27.07 1.65.07 4.85s-.01 3.58-.07 4.85c-.05 1.17-.25 1.8-.41 2.23-.22.56-.48.96-.9 1.38-.42.42-.82.68-1.38.9-.42.16-1.06.36-2.23.41-1.27.06-1.65.07-4.85.07s-3.58-.01-4.85-.07c-1.17-.05-1.8-.25-2.23-.41a3.72 3.72 0 0 1-1.38-.9 3.72 3.72 0 0 1-.9-1.38c-.16-.42-.36-1.06-.41-2.23-.06-1.27-.07-1.65-.07-4.85s.01-3.58.07-4.85c.05-1.17.25-1.8.41-2.23.22-.56.48-.96.9-1.38.42-.42.82-.68 1.38-.9.42-.16 1.06-.36 2.23-.41C8.42 2.17 8.8 2.16 12 2.16ZM12 0C8.74 0 8.33.01 7.05.07 5.78.13 4.9.33 4.14.63c-.79.3-1.46.72-2.12 1.38A5.86 5.86 0 0 0 .63 4.14c-.3.76-.5 1.64-.56 2.91C.01 8.33 0 8.74 0 12s.01 3.67.07 4.95c.06 1.27.26 2.15.56 2.91.3.79.72 1.46 1.38 2.12.66.66 1.33 1.08 2.12 1.38.76.3 1.64.5 2.91.56C8.33 23.99 8.74 24 12 24s3.67-.01 4.95-.07c1.27-.06 2.15-.26 2.91-.56a5.86 5.86 0 0 0 2.12-1.38 5.86 5.86 0 0 0 1.38-2.12c.3-.76.5-1.64.56-2.91.06-1.28.07-1.69.07-4.95s-.01-3.67-.07-4.95c-.06-1.27-.26-2.15-.56-2.91a5.86 5.86 0 0 0-1.38-2.12A5.86 5.86 0 0 0 19.86.63c-.76-.3-1.64-.5-2.91-.56C15.67.01 15.26 0 12 0Zm0 5.84A6.16 6.16 0 1 0 18.16 12 6.16 6.16 0 0 0 12 5.84ZM12 16a4 4 0 1 1 4-4 4 4 0 0 1-4 4Zm6.41-10.85a1.44 1.44 0 1 0 1.44 1.44 1.44 1.44 0 0 0-1.44-1.44Z"></path></svg></a>
      <a href="#" aria-label="TikTok"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12.525.02c1.31-.02 2.61-.01 3.91-.02.08 1.53.63 3.09 1.75 4.17 1.12 1.11 2.7 1.62 4.24 1.79v4.03c-1.44-.05-2.89-.35-4.2-.97-.57-.26-1.1-.59-1.62-.93-.01 2.92.01 5.84-.02 8.75-.08 1.4-.54 2.79-1.35 3.94-1.31 1.92-3.58 3.17-5.91 3.21-1.43.08-2.86-.31-4.08-1.03-2.02-1.19-3.44-3.37-3.65-5.71-.02-.5-.03-1-.01-1.49.18-1.9 1.12-3.72 2.58-4.96 1.66-1.44 3.98-2.13 6.15-1.72.02 1.48-.04 2.96-.04 4.44-.99-.32-2.15-.23-3.02.37-.63.41-1.11 1.04-1.36 1.75-.21.51-.15 1.07-.14 1.61.24 1.64 1.82 3.02 3.5 2.87 1.12-.01 2.19-.66 2.77-1.61.19-.33.4-.67.41-1.06.1-1.79.06-3.57.07-5.36.01-4.03-.01-8.05.02-12.07Z"></path></svg></a>
    </div>
    <nav class="hbo-flinks">
      <a href="#">Acessibilidade</a>
      <a href="#">Política de Privacidade</a>
      <a href="#">Termos de Uso</a>
      <a href="#">Gerir cookies</a>
      <a href="#">Informações</a>
      <a href="#">Ajuda</a>
    </nav>
    <p class="hbo-copy">© 2026 bho MAX. Todos os direitos reservados. (marca fictícia — sem afiliação real)</p>
  </footer>

  <button class="hbo-chat" type="button" aria-label="Abrir chat de ajuda">
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M7.9 20A9 9 0 1 0 4 16.1L2 22z"></path></svg>
  </button>
</body>
</html>`;

// Interface simulada "NetsFlix - Acesse sua conta" (login/captura) — paródia FICTÍCIA
// (compliance de IP: sem logo/fita curvada, sem "Netflix" no texto; logo = "N" plano em
// CSS #E50914). Consolidação do clone Next.js + MUI + Tailwind originalmente em ".Pagina"
// (Hero): os
// componentes React e o estilo utilitário foram unificados numa ÚNICA string de
// HTML com CSS embutido (Tailwind não compila neste frontend — ver a nota do HBO).
//
// O colagem de fundo (netflix-bg.png) foi copiada para `public/netflix-bg.png` e é
// referenciada por caminho absoluto `/netflix-bg.png` — resolvido contra a origem do
// app tanto no preview (iframe srcDoc) quanto na landing servida (/landing/:id).
//
// Telemetria: mesmo padrão do bho MAX. O <form> intercepta o submit via `onsubmit`
// inline, dispara o gatilho de rastreamento (POST /api/tracking/submit/...) enviando
// apenas flags de validação — NUNCA a senha em texto (LGPD) — e então redireciona
// para a rota educacional interna (/educational-feedback?template=basico_phishing).
const netflixLoginHtml = `<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>NetsFlix</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; -webkit-font-smoothing: antialiased; }
  .nfx-root { position: relative; min-height: 100vh; width: 100%; overflow: hidden; background-color: #000; }
  /* Colagem de fundo: blur + brightness MODERADOS — suaviza os pôsteres (ainda dá p/
     perceber alguns), sem escurecer a tela por completo. O scale(1.1) evita que o blur
     revele as bordas transparentes do container. */
  .nfx-bg { position: absolute; inset: 0; background-image: url('/netflix-bg.png'); background-size: cover; background-position: center; filter: blur(8px) brightness(0.8); transform: scale(1.1); }
  .nfx-overlay { position: absolute; inset: 0; background: rgba(0,0,0,0.5); }
  .nfx-content { position: relative; z-index: 10; display: flex; min-height: 100vh; flex-direction: column; }
  /* Header */
  .nfx-header { display: flex; align-items: center; justify-content: space-between; padding: 20px 24px; }
  /* Logo parodiado: letra "N" plana (2D), Arial Black / geométrica, vermelho de marca,
     leve escala vertical p/ manter o peso visual — sem imagem/SVG da fita curvada. */
  .nfx-logo { user-select: none; display: inline-block; font-family: 'Arial Black', 'Helvetica Neue', Arial, sans-serif; font-size: 2.25rem; font-weight: 900; line-height: 1; letter-spacing: -0.02em; color: #E50914; transform: scaleY(1.1); }
  @media (min-width: 768px) { .nfx-header { padding: 20px 48px; } .nfx-logo { font-size: 2.6rem; } }
  /* Hero */
  .nfx-hero { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 24px 24px 96px; text-align: center; }
  .nfx-title { max-width: 42rem; font-size: 2.25rem; font-weight: 800; line-height: 1.15; color: #fff; }
  @media (min-width: 768px) { .nfx-title { font-size: 3.75rem; } }
  .nfx-price { margin-top: 20px; font-size: 1.125rem; font-weight: 500; color: #fff; }
  @media (min-width: 768px) { .nfx-price { font-size: 1.25rem; } }
  .nfx-prompt { margin-top: 24px; font-size: 1rem; color: #fff; }
  @media (min-width: 768px) { .nfx-prompt { font-size: 1.125rem; } }
  /* Formulário */
  .nfx-form { margin-top: 20px; width: 100%; max-width: 36rem; display: flex; flex-direction: column; align-items: center; gap: 16px; }
  .nfx-field { position: relative; width: 100%; }
  .nfx-input {
    width: 100%; background: rgba(22,22,22,0.7); color: #fff; font-size: 16px; font-family: inherit;
    padding: 18px 14px 18px 14px; border-radius: 4px; border: 1px solid rgba(128,128,128,0.7);
    outline: none; transition: border-color .15s ease;
  }
  .nfx-input::placeholder { color: rgba(255,255,255,0.7); }
  .nfx-input:hover { border-color: rgba(255,255,255,0.8); }
  .nfx-input:focus { border-color: #fff; }
  .nfx-btn {
    display: inline-flex; align-items: center; justify-content: center; gap: 8px; cursor: pointer;
    background: #e50914; color: #fff; font-family: inherit; font-size: 1.25rem; font-weight: 700;
    text-transform: none; border: none; border-radius: 4px; padding: 12px 32px;
  }
  .nfx-btn:hover { background: #f6121d; }
  .nfx-btn svg { width: 22px; height: 22px; }
</style>
</head>
<body>
  <div class="nfx-root">
    <div class="nfx-bg" aria-hidden="true"></div>
    <div class="nfx-overlay" aria-hidden="true"></div>
    <div class="nfx-content">
      <header class="nfx-header">
        <span class="nfx-logo">N</span>
      </header>
      <section class="nfx-hero">
        <h1 class="nfx-title">Filmes, séries e muito mais, sem limites</h1>
        <p class="nfx-price">A partir de R$ 20,90. Cancele quando quiser.</p>
        <p class="nfx-prompt">Quer assistir? Informe seu email e senha para criar ou entrar em sua conta.</p>

        <form class="nfx-form" onsubmit="event.preventDefault();var em=document.getElementById('nfx-email').value;var pw=document.getElementById('nfx-password').value;var meta={camposPreenchidos:(em.length>0&&pw.length>0),emailInformado:(em.length>0),tamanhoSenha:pw.length};fetch('/api/tracking/submit/{{CAMPAIGN_ID}}/{{TARGET_ID}}',{method:'POST',headers:{'Content-Type':'application/json','ngrok-skip-browser-warning':'true'},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href='/educational-feedback?template=netsflix&c={{CAMPAIGN_ID}}&t={{TARGET_ID}}';});return false;">
          <div class="nfx-field">
            <input class="nfx-input" id="nfx-email" name="email" type="email" placeholder="Email" autocomplete="email" required>
          </div>
          <div class="nfx-field">
            <input class="nfx-input" id="nfx-password" name="password" type="password" placeholder="Senha" autocomplete="current-password" required>
          </div>
          <button type="submit" class="nfx-btn">
            Vamos lá
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m9 18 6-6-6-6"></path></svg>
          </button>
        </form>
      </section>
    </div>
  </div>
</body>
</html>`;

// Interface simulada "amzprime - Alterar Senha" (login/captura) — paródia FICTÍCIA da
// Amazon p/ compliance de IP (sem logo/marca/dados cadastrais reais). Consolidação do
// clone Next.js originalmente em ".pagina" (Header + ChangePasswordForm + Footer).
//
// ⚠️ AUTO-CONTIDO (correção 2026-07-17): antes esta era a ÚNICA landing que dependia
// do Tailwind Play CDN (<script src="cdn.tailwindcss.com">) para estilizar. No
// PREVIEW (Templates.tsx → iframe com sandbox="allow-same-origin" SEM allow-scripts,
// por segurança/XSS) esse script NÃO executa, então nenhuma classe utilitária era
// gerada e a página caía no estilo padrão do navegador — todos os <a> viravam texto
// azul sublinhado e o layout colapsava. Agora seguimos o mesmo padrão do HBO/Netflix:
// um <style> EMBUTIDO com CSS puro e classes escopadas (.amz-*), zero JS/CDN. Renderiza
// idêntico nos dois contextos:
//  - Preview em `iframe srcDoc`: o <style> aplica dentro do documento isolado do iframe.
//  - Landing servida `/landing/:id` (dangerouslySetInnerHTML): rota standalone; o <style>
//    é a única fonte de estilo da página (o React ignora <script> injetado, então nem
//    precisamos mais do CDN). O reset `a{text-decoration:none;color:inherit}` mata de vez
//    o "tudo azul sublinhado".
//
// Sem imagens externas: o logo "amzprime" (wordmark parodiado, fonte de design em
// .logoFalsa/) é textual e todos os ícones são SVG inline (sem caminhos de arquivo
// que possam quebrar). Paródia p/ compliance de IP: sem marca/dados cadastrais reais.
//
// Telemetria: mesmo padrão do HBO/Netflix — <form onsubmit> inline dispara o
// gatilho de rastreamento (só flags de validação, NUNCA a senha — LGPD) e então
// redireciona para /educational-feedback?template=basico_phishing.
const amazonLoginHtml = `<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Alterar Senha - amzprime</title>
<style>
  @import url('https://fonts.googleapis.com/css2?family=Nunito+Sans:wght@400;700;800&display=swap');
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { min-height: 100vh; display: flex; flex-direction: column; background: #ffffff; color: #0f1111; font-family: 'Nunito Sans', Arial, Helvetica, sans-serif; -webkit-font-smoothing: antialiased; }
  /* Reset dos links — SEM isto o navegador pinta tudo de azul sublinhado no preview. */
  a { color: inherit; text-decoration: none; }
  svg { display: block; }
  button { font-family: inherit; cursor: pointer; }
  input { font-family: inherit; }

  /* ---------- Wordmark parodiado "amzprime" (fonte de design em .logoFalsa/) ----------
     Nunito Sans (livre) no lugar do Amazon Ember. Em fundo ESCURO (header/rodapé desta
     landing) "amz" vai em branco por legibilidade — no e-mail (fundo branco) "amz" usa
     #232F3E. "prime" fica sempre no azul-marca #00A8E1 (gatilho cognitivo). */
  .amz-wordmark { font-family: 'Nunito Sans', Arial, sans-serif; font-weight: 800; letter-spacing: -1px; line-height: 1; }
  .amz-wordmark .w-amz { color: #ffffff; }
  .amz-wordmark .w-prime { color: #00A8E1; }

  /* ---------- Header ---------- */
  .amz-header { background: #131921; color: #fff; }
  .amz-top { display: flex; align-items: center; gap: 6px; padding: 6px 12px; flex-wrap: wrap; }
  .amz-box { display: flex; align-items: flex-end; padding: 6px 8px; border: 1px solid transparent; border-radius: 2px; background: transparent; color: #fff; text-align: left; line-height: 1.15; }
  .amz-box:hover { border-color: #fff; }
  .amz-logo b { font-size: 22px; font-weight: 800; letter-spacing: -.5px; }
  .amz-logo .tld { font-size: 11px; color: #d5d5d5; margin: 0 0 3px 2px; }
  .amz-search { display: flex; flex: 1 1 260px; min-width: 200px; height: 40px; border-radius: 6px; overflow: hidden; margin: 0 4px; }
  .amz-search .cat { display: flex; align-items: center; gap: 4px; background: #e6e6e6; color: #555; font-size: 12px; padding: 0 10px; white-space: nowrap; }
  .amz-search input { flex: 1; min-width: 0; border: 0; padding: 0 12px; font-size: 14px; color: #111; outline: none; }
  .amz-search .go { width: 45px; border: 0; background: #febd69; display: flex; align-items: center; justify-content: center; }
  .amz-search .go:hover { background: #f3a847; }
  .amz-act .s1 { font-size: 12px; color: #ccc; }
  .amz-act .s2 { font-size: 13px; font-weight: 700; display: flex; align-items: center; gap: 2px; }
  .amz-cart { display: flex; align-items: flex-end; gap: 2px; }
  .amz-cart .badge { align-self: flex-start; color: #f08804; font-size: 13px; font-weight: 700; margin-left: -6px; }

  .amz-nav { display: flex; align-items: center; gap: 2px; background: #232f3e; padding: 4px 8px; font-size: 13px; overflow-x: auto; }
  .amz-nav a, .amz-nav .ham { display: flex; align-items: center; gap: 4px; color: #fff; background: transparent; border: 1px solid transparent; border-radius: 2px; padding: 4px 8px; white-space: nowrap; font-size: 13px; }
  .amz-nav .ham { font-weight: 700; }
  .amz-nav a:hover, .amz-nav .ham:hover { border-color: #fff; }

  /* ---------- Main / formulário ---------- */
  .amz-main { flex: 1; display: flex; justify-content: center; padding: 32px 16px; }
  .amz-col { width: 100%; max-width: 480px; }
  .amz-h1 { font-size: 28px; font-weight: 400; margin-bottom: 14px; }
  .amz-card { border: 1px solid #ddd; border-radius: 8px; padding: 20px; }
  .amz-lead { font-size: 13px; margin-bottom: 16px; }
  .amz-field { margin-bottom: 16px; }
  .amz-field label { display: block; font-size: 13px; font-weight: 700; margin-bottom: 4px; }
  .amz-field input { width: 100%; max-width: 270px; height: 31px; border: 1px solid #a6a6a6; border-radius: 8px; padding: 0 8px; font-size: 14px; box-shadow: inset 0 1px 2px rgba(15,17,17,.15); outline: none; }
  .amz-field input:focus { border-color: #e77600; box-shadow: 0 0 3px 2px rgba(228,121,17,.5); }
  .amz-save { margin-top: 4px; border: 1px solid #a88734; border-radius: 20px; background: #f7ca00; padding: 7px 20px; font-size: 13px; color: #111; box-shadow: 0 2px 5px rgba(213,217,217,.5); }
  .amz-save:hover { background: #f2c200; }
  .amz-help { margin-top: 20px; font-size: 13px; }
  .amz-help .b { font-weight: 700; }
  .amz-help a { color: #007185; }
  .amz-help a:hover { color: #c7511f; text-decoration: underline; }

  /* ---------- Footer ---------- */
  .amz-back { width: 100%; border: 0; background: #37475a; color: #fff; font-size: 13px; font-weight: 700; padding: 15px; margin-top: 32px; }
  .amz-back:hover { background: #485769; }
  .amz-foot { background: #232f3e; color: #fff; padding: 36px 24px; }
  .amz-grid { max-width: 900px; margin: 0 auto; display: grid; grid-template-columns: repeat(2, 1fr); gap: 28px; }
  .amz-grid h2 { font-size: 15px; margin-bottom: 8px; font-weight: 700; }
  .amz-grid ul { list-style: none; display: flex; flex-direction: column; gap: 8px; }
  .amz-grid a { font-size: 13px; color: #ddd; }
  .amz-grid a:hover { text-decoration: underline; }
  .amz-brand { margin-top: 32px; display: flex; align-items: center; justify-content: center; gap: 20px; }
  .amz-brand .name { font-size: 21px; font-weight: 700; }
  .amz-brand .name sup { font-size: 10px; }
  .amz-region { display: flex; align-items: center; gap: 8px; border: 1px solid #8d919b; border-radius: 2px; padding: 6px 12px; font-size: 13px; background: transparent; color: #fff; }
  .amz-region:hover { border-color: #fff; }
  .amz-legal { background: #131a22; color: #ddd; text-align: center; padding: 28px 24px; }
  .amz-legal .links { display: flex; flex-wrap: wrap; align-items: center; justify-content: center; gap: 4px 16px; font-size: 11px; margin-bottom: 12px; }
  .amz-legal .links a:hover { text-decoration: underline; }
  .amz-legal p { font-size: 11px; color: #999; }
  .amz-legal p + p { margin-top: 20px; }

  @media (min-width: 768px) { .amz-grid { grid-template-columns: repeat(4, 1fr); } }
  @media (max-width: 640px) { .amz-hide-sm { display: none; } }
</style>
</head>
<body>
  <header class="amz-header">
    <div class="amz-top">
      <a href="#" class="amz-box amz-logo" aria-label="amzprime">
        <span class="amz-wordmark" style="font-size:24px;"><span class="w-amz">amz</span><span class="w-prime">prime</span></span>
      </a>
      <div class="amz-search">
        <span class="cat">Todos <svg viewBox="0 0 24 24" width="14" height="14" fill="currentColor" aria-hidden="true"><path d="M7 10l5 5 5-5z"></path></svg></span>
        <input placeholder="Pesquisar amzprime" aria-label="Pesquisar">
        <button class="go" aria-label="Pesquisar"><svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="#111" stroke-width="2" aria-hidden="true"><circle cx="11" cy="11" r="8"></circle><path d="m21 21-4.3-4.3"></path></svg></button>
      </div>
      <button class="amz-box amz-act amz-hide-sm">
        <span class="s1">Olá, faça seu login</span>
        <span class="s2">Contas e Listas <svg viewBox="0 0 24 24" width="14" height="14" fill="#ccc" aria-hidden="true"><path d="M7 10l5 5 5-5z"></path></svg></span>
      </button>
      <button class="amz-box amz-act amz-hide-sm">
        <span class="s1">Devoluções</span>
        <span class="s2">e Pedidos</span>
      </button>
      <button class="amz-box amz-cart" aria-label="Carrinho">
        <span style="position:relative;display:inline-flex;">
          <svg viewBox="0 0 24 24" width="30" height="30" fill="currentColor" aria-hidden="true"><path d="M7 18c-1.1 0-1.99.9-1.99 2S5.9 22 7 22s2-.9 2-2-.9-2-2-2zM1 2v2h2l3.6 7.59-1.35 2.45c-.16.28-.25.61-.25.96 0 1.1.9 2 2 2h12v-2H7.42c-.14 0-.25-.11-.25-.25l.03-.12L8.1 13h7.45c.75 0 1.41-.41 1.75-1.03l3.58-6.49A1 1 0 0 0 20 4H5.21l-.94-2H1zm16 16c-1.1 0-1.99.9-1.99 2s.89 2 1.99 2 2-.9 2-2-.9-2-2-2z"></path></svg>
          <span class="badge">0</span>
        </span>
        <span style="font-size:13px;font-weight:700;">Carrinho</span>
      </button>
    </div>
    <nav class="amz-nav">
      <span class="ham"><svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor" aria-hidden="true"><path d="M3 18h18v-2H3v2zm0-5h18v-2H3v2zm0-7v2h18V6H3z"></path></svg> Todos</span>
      <a href="#">Venda na amzprime</a>
      <a href="#">Prime</a>
      <a href="#">Ofertas do Dia</a>
      <a href="#">Comprar novamente</a>
      <a href="#">Ideias de Presente</a>
      <a href="#">Atendimento ao Cliente</a>
      <a href="#">Sua amzprime</a>
      <a href="#">eBooks Kindle</a>
      <a href="#">Mais Vendidos</a>
      <a href="#">Livros</a>
    </nav>
  </header>

  <main class="amz-main">
    <div class="amz-col">
      <h1 class="amz-h1">Alterar Senha</h1>
      <div class="amz-card">
        <p class="amz-lead">Use o formulário a seguir para alterar a senha de sua conta amzprime</p>
        <form onsubmit="event.preventDefault();var np=document.getElementById('amz-new').value;var cp=document.getElementById('amz-confirm').value;var meta={camposPreenchidos:(np.length>0&&cp.length>0),senhasCoincidem:(np===cp&&np.length>0),tamanhoSenha:np.length};fetch('/api/tracking/submit/{{CAMPAIGN_ID}}/{{TARGET_ID}}',{method:'POST',headers:{'Content-Type':'application/json','ngrok-skip-browser-warning':'true'},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href='/educational-feedback?template=amzprime&c={{CAMPAIGN_ID}}&t={{TARGET_ID}}';});return false;">
          <div class="amz-field">
            <label for="amz-new">Senha nova:</label>
            <input id="amz-new" name="new-password" type="password" autocomplete="new-password" required>
          </div>
          <div class="amz-field">
            <label for="amz-confirm">Reinsira a nova senha:</label>
            <input id="amz-confirm" name="confirm-password" type="password" autocomplete="new-password" required>
          </div>
          <button type="submit" class="amz-save">Salvar alterações</button>
        </form>
        <div class="amz-help">
          <p class="b">Dispositivo perdido ou roubado? Atividade incomum?</p>
          <p>Em vez disso, <a href="#">Proteja sua conta</a></p>
        </div>
      </div>
    </div>
  </main>

  <footer>
    <button class="amz-back">Voltar ao início</button>
    <div class="amz-foot">
      <div class="amz-grid">
        <div>
          <h2>Conheça-nos</h2>
          <ul>
            <li><a href="#">Sobre a amzprime</a></li>
            <li><a href="#">Informações corporativas</a></li>
            <li><a href="#">Carreiras</a></li>
            <li><a href="#">Comunicados à imprensa</a></li>
            <li><a href="#">Acessibilidade</a></li>
          </ul>
        </div>
        <div>
          <h2>Ganhe dinheiro conosco</h2>
          <ul>
            <li><a href="#">Venda na amzprime</a></li>
            <li><a href="#">Forneça para a amzprime</a></li>
            <li><a href="#">Publique seus livros</a></li>
            <li><a href="#">Seja um associado</a></li>
            <li><a href="#">Anuncie seus produtos</a></li>
          </ul>
        </div>
        <div>
          <h2>Pagamento</h2>
          <ul>
            <li><a href="#">Meios de Pagamento</a></li>
            <li><a href="#">Compre com Pontos</a></li>
            <li><a href="#">Cartão de crédito amzprime</a></li>
          </ul>
        </div>
        <div>
          <h2>Deixe-nos ajudar você</h2>
          <ul>
            <li><a href="#">Sua conta</a></li>
            <li><a href="#">Frete e prazo de entrega</a></li>
            <li><a href="#">Devoluções e reembolsos</a></li>
            <li><a href="#">Ajuda</a></li>
          </ul>
        </div>
      </div>
      <div class="amz-brand">
        <span class="amz-wordmark" style="font-size:21px;"><span class="w-amz">amz</span><span class="w-prime">prime</span></span>
        <button class="amz-region">
          <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><circle cx="12" cy="12" r="10"></circle><path d="M2 12h20"></path><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path></svg>
          Brasil
        </button>
      </div>
    </div>
    <div class="amz-legal">
      <div class="links">
        <a href="#">Condições de Uso</a>
        <a href="#">Notificação de Privacidade</a>
        <a href="#">Cookies</a>
        <a href="#">Anúncios Baseados em Interesses</a>
      </div>
      <p>© 2021-2026 amzprime. Todos os direitos reservados.</p>
      <p>amzprime Serviços Digitais Ltda. | CNPJ 32.869.296/0652-65</p>
    </div>
  </footer>
</body>
</html>`;

// ---------------------------------------------------------------------------
// LANDING FICTÍCIA "Microsft 365" — página falsa de login corporativo (par da isca de
// e-mail 'microcorp-expiracao-senha' -> cenário 'cenario-microcorp'). Consolidada em UMA
// string HTML com CSS embutido (as demais landings seguem o mesmo padrão; classes
// Tailwind não compilam no HTML injetado por dangerouslySetInnerHTML nem no preview em
// iframe). SEM propriedade intelectual real: identidade é o logotipo-paródia (grid 2x2 de
// 4 quadrados coloridos em tons ADAPTADOS) + wordmark "Microsft 365" (typosquatting
// proposital — sem o segundo "o"), espelhando o e-mail. O slug do id permanece
// 'microcorp-login' por estabilidade (renomear quebraria campanhas legadas).
// Telemetria: mesmo padrão de HBO/Netflix/Amazon — <form onsubmit> inline
// (funciona sob dangerouslySetInnerHTML, ao contrário de <script>) dispara
// POST /api/tracking/submit/{{CAMPAIGN_ID}}/{{TARGET_ID}} enviando APENAS metadados de
// validação (flags + tamanho) — NUNCA e-mail/senha reais (LGPD) — e redireciona para o
// treinamento interativo /educational-feedback?template=microsft365 (com c/t p/ auditoria).
// ---------------------------------------------------------------------------
const microCorpLoginHtml = `<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Recuperar sua conta | Microsft 365</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body {
    position: relative;
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 16px;
    background: #f5f5f5;
    color: #1b1b1b;
    font-family: 'Segoe UI', system-ui, -apple-system, Roboto, Arial, sans-serif;
    -webkit-font-smoothing: antialiased;
  }
  .mc-card {
    width: 100%;
    max-width: 440px;
    background: #ffffff;
    padding: 36px 44px;
    box-shadow: 0 2px 6px rgba(0,0,0,0.2);
  }
  .mc-brand { display: flex; align-items: center; gap: 8px; }
  .mc-logo { display: grid; grid-template-columns: 11px 11px; grid-template-rows: 11px 11px; gap: 2px; }
  .mc-logo i { display: block; width: 11px; height: 11px; }
  .mc-brand span { font-size: 15px; font-weight: 600; color: #5e5e5e; }
  .mc-title { margin-top: 24px; font-size: 24px; font-weight: 600; line-height: 1.2; color: #1b1b1b; }
  .mc-desc { margin-top: 16px; font-size: 15px; line-height: 1.4; color: #1b1b1b; }
  .mc-field { margin-top: 20px; }
  .mc-input {
    width: 100%;
    padding: 0 0 6px 0;
    font-size: 15px;
    color: #1b1b1b;
    background: transparent;
    border: none;
    border-bottom: 1px solid #666666;
    outline: none;
  }
  .mc-input::placeholder { color: #767676; }
  .mc-input:focus { border-bottom: 2px solid #0067b8; }
  .mc-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 36px; }
  .mc-btn {
    min-width: 108px;
    height: 32px;
    padding: 0 16px;
    font-size: 15px;
    border: none;
    cursor: pointer;
    transition: background 0.15s ease;
  }
  .mc-btn-secondary { background: #e6e6e6; color: #1b1b1b; }
  .mc-btn-secondary:hover { background: #dadada; }
  .mc-btn-primary { background: #0067b8; color: #ffffff; }
  .mc-btn-primary:hover { background: #005da6; }
  .mc-foot {
    position: absolute;
    right: 0;
    bottom: 0;
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 4px;
    padding: 12px 16px;
    font-size: 13px;
    color: #1b1b1b;
  }
  .mc-foot-links { display: flex; gap: 20px; }
  .mc-foot a { color: inherit; text-decoration: none; }
  .mc-foot a:hover { text-decoration: underline; }
  .mc-foot a.mc-link { color: #0067b8; }
</style>
</head>
<body>
  <div class="mc-card">
    <div class="mc-brand">
      <div class="mc-logo" aria-hidden="true">
        <i style="background:#e8452a"></i>
        <i style="background:#6faf12"></i>
        <i style="background:#1b9de0"></i>
        <i style="background:#f5a800"></i>
      </div>
      <span>Microsft 365</span>
    </div>

    <h1 class="mc-title">Recuperar sua conta</h1>

    <p class="mc-desc">Podemos ajudá-lo a redefinir sua senha e informações de segurança. Primeiro, insira sua senha e e-mail da conta institucional e siga as instruções a seguir.</p>

    <!-- Telemetria segura: intercepta o submit, envia SOMENTE metadados (flags/tamanho),
         nunca e-mail/senha reais, e encaminha para o treinamento de conscientização. -->
    <form class="mc-form"
      onsubmit="event.preventDefault();var qs=new URLSearchParams(window.location.search);var c='{{CAMPAIGN_ID}}'||qs.get('c')||'';var t='{{TARGET_ID}}'||qs.get('t')||'';var e=(document.getElementById('mc-email')||{}).value||'';var p=(document.getElementById('mc-password')||{}).value||'';var meta={camposPreenchidos:(e.length>0&&p.length>0),senhasCoincidem:true,tamanhoSenha:p.length};fetch('/api/tracking/submit/'+c+'/'+t,{method:'POST',headers:{'Content-Type':'application/json','ngrok-skip-browser-warning':'true'},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href='/educational-feedback?template=microsft365&c='+encodeURIComponent(c)+'&t='+encodeURIComponent(t);});return false;">

      <div class="mc-field">
        <input class="mc-input" id="mc-email" name="email" type="text" autocomplete="username" placeholder="Email" aria-label="Email" required>
      </div>

      <div class="mc-field">
        <input class="mc-input" id="mc-password" name="password" type="password" autocomplete="current-password" placeholder="Senha" aria-label="Senha" required>
      </div>

      <div class="mc-actions">
        <button type="button" class="mc-btn mc-btn-secondary" onclick="window.location.href='/educational-feedback?template=microsft365&c={{CAMPAIGN_ID}}&t={{TARGET_ID}}';">Cancelar</button>
        <button type="submit" class="mc-btn mc-btn-primary">Avançar</button>
      </div>
    </form>
  </div>

  <footer class="mc-foot">
    <div class="mc-foot-links">
      <a href="#">Termos de uso</a>
      <a href="#">Privacidade e cookies</a>
    </div>
    <p>Use a navegação privada se esse não for seu dispositivo. <a href="#" class="mc-link">Saiba mais</a></p>
  </footer>
</body>
</html>`;

// Pagina Simulada (login/redefinicao de senha) do cenario "Mercado Liv".
// Reproduz o layout de referencia (.paginaSimulada/mercado-liv): header amarelo
// #FEE501, logo caixa-baixa "mercado/liv" em azul #2E347E, grid responsivo 2 col
// (mobile: 1 col) e card de login. CSS embutido (Tailwind NAO e processado quando
// o HTML e injetado via dangerouslySetInnerHTML/srcDoc). Telemetria: <form onsubmit>
// inline dispara POST /api/tracking/submit/{{CAMPAIGN_ID}}/{{TARGET_ID}} (so metadados,
// nunca credenciais reais - LGPD) e redireciona a Tela Educacional do cenario.
const mercadoLivLoginHtml = `<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Mercado Liv - Acesse sua conta</title>
<style>
  @import url('https://fonts.googleapis.com/css2?family=Nunito:wght@400;600;700;800&display=swap');
  *{box-sizing:border-box;margin:0;padding:0}
  :root{--ml-yellow:#FEE501;--ml-blue:#2E347E;--ml-fg:#1a1a2e;--ml-border:#e6e6e6;--ml-muted-fg:#737373}
  body{min-height:100vh;display:flex;flex-direction:column;background:#fff;color:var(--ml-fg);font-family:'Nunito',system-ui,-apple-system,'Segoe UI',Roboto,sans-serif;-webkit-font-smoothing:antialiased}
  a{color:inherit;text-decoration:none}
  /* Header amarelo (#FEE501) com a logo caixa-baixa "mercado/liv" em azul (#2E347E) */
  .ml-header{background:var(--ml-yellow)}
  .ml-header-inner{max-width:1200px;margin:0 auto;padding:12px 24px;display:flex;align-items:center}
  .ml-logo{display:flex;align-items:center;gap:8px}
  .ml-logo svg{display:block;width:52px;height:38px}
  .ml-logo-word{font-size:20px;font-weight:800;line-height:.92;color:var(--ml-blue);text-transform:lowercase;letter-spacing:-.5px}
  /* Conteudo - grid responsivo (1 col mobile, 2 cols >=768px) */
  .ml-main{flex:1}
  .ml-main-inner{max-width:1200px;margin:0 auto;padding:40px 24px;display:grid;grid-template-columns:1fr;gap:40px}
  @media(min-width:768px){.ml-main-inner{grid-template-columns:1fr 1fr;gap:32px;padding:72px 24px}}
  .ml-left{display:flex;flex-direction:column}
  .ml-title{max-width:28rem;font-size:1.75rem;font-weight:700;line-height:1.2;color:var(--ml-fg)}
  @media(min-width:768px){.ml-title{font-size:2.35rem}}
  .ml-help{margin-top:40px;max-width:28rem}
  .ml-secbtn{display:flex;width:100%;align-items:center;gap:12px;border:1px solid var(--ml-border);background:#fff;padding:16px;border-radius:8px;box-shadow:0 1px 2px rgba(0,0,0,.06);text-align:left;cursor:pointer;font:inherit;color:inherit}
  .ml-secbtn:hover{background:#f7f7f7}
  .ml-secbtn>svg{width:26px;height:26px;color:var(--ml-fg);flex-shrink:0}
  .ml-secbtn .ml-sec-txt{flex:1;font-size:15px}
  .ml-secbtn .ml-chev{width:20px;height:20px;color:var(--ml-muted-fg)}
  .ml-help-link{margin-top:24px;display:inline-block;font-size:15px;font-weight:600;color:var(--ml-blue)}
  .ml-help-link:hover{text-decoration:underline}
  /* Card de login */
  .ml-right{display:flex;justify-content:center}
  @media(min-width:768px){.ml-right{justify-content:flex-end}}
  .ml-card{width:100%;max-width:490px;border:1px solid var(--ml-border);background:#fff;border-radius:8px;padding:32px;box-shadow:0 1px 2px rgba(0,0,0,.08)}
  .ml-label{display:block;margin-bottom:6px;font-size:15px;color:var(--ml-fg)}
  .ml-input{height:48px;width:100%;margin-bottom:20px;border:1px solid var(--ml-blue);border-radius:6px;padding:0 12px;font-size:15px;font-family:inherit;color:var(--ml-fg);outline:none;background:#fff;transition:box-shadow .15s}
  .ml-input:focus{box-shadow:0 0 0 3px rgba(46,52,126,.2)}
  .ml-btn{height:48px;width:100%;border:0;border-radius:6px;font-size:15px;font-weight:700;font-family:inherit;cursor:pointer}
  .ml-btn-primary{background:var(--ml-blue);color:#fff;transition:background .15s}
  .ml-btn-primary:hover{background:#262b6a}
  .ml-btn-ghost{margin-top:16px;background:transparent;color:var(--ml-blue);transition:background .15s}
  .ml-btn-ghost:hover{background:rgba(46,52,126,.06)}
  .ml-divider{display:flex;align-items:center;gap:12px;margin:16px 0}
  .ml-divider .line{height:1px;flex:1;background:var(--ml-border)}
  .ml-divider .txt{font-size:13px;color:var(--ml-muted-fg)}
  .ml-google{display:flex;height:48px;width:100%;align-items:center;justify-content:center;gap:12px;border:1px solid var(--ml-border);background:#fff;border-radius:6px;font-size:15px;font-family:inherit;color:var(--ml-fg);cursor:pointer}
  .ml-google:hover{background:#f7f7f7}
  /* Footer */
  .ml-footer{background:#f5f5f5}
  .ml-footer-inner{max-width:1200px;margin:0 auto;padding:16px 24px;display:flex;flex-direction:column;gap:8px;font-size:13px;color:var(--ml-muted-fg)}
  @media(min-width:768px){.ml-footer-inner{flex-direction:row;align-items:center;justify-content:space-between}}
  .ml-footer a{color:var(--ml-blue)}
  .ml-footer a:hover{text-decoration:underline}
  .ml-recaptcha{display:flex;flex-wrap:wrap;gap:4px;align-items:center}
  .ml-recaptcha a{color:var(--ml-fg);font-weight:600}
</style>
</head>
<body>
  <!-- CABECALHO amarelo (#FEE501) + logo "mercado/liv" (caixa baixa, azul #2E347E) -->
  <header class="ml-header">
    <div class="ml-header-inner">
      <div class="ml-logo">
        <svg width="52" height="38" viewBox="0 0 52 38" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
          <ellipse cx="26" cy="19" rx="25" ry="16" fill="#FFF1B8" stroke="#2E347E" stroke-width="1.5"/>
          <path d="M14 15c2.5-2.5 5.5-2.5 8 0l4 4 4-4c2.5-2.5 5.5-2.5 8 0" stroke="#2E347E" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
          <path d="M18 19.5c1.6 1.8 3.4 3.2 5.2 4.4 1.6 1.1 3.4 1.1 5 0M22 26c1.4 1 2.9 1.6 4.4 1.7" stroke="#2E347E" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
        </svg>
        <span class="ml-logo-word">mercado<br>liv</span>
      </div>
    </div>
  </header>

  <main class="ml-main">
    <div class="ml-main-inner">
      <!-- Coluna esquerda: chamada + atalhos (links de destaque em azul #2E347E) -->
      <div class="ml-left">
        <h1 class="ml-title">Digite seu e-mail e senha atual para alterar sua senha</h1>
        <div class="ml-help">
          <button type="button" class="ml-secbtn">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 3l7 3v5c0 4.5-3 8-7 10-4-2-7-5.5-7-10V6l7-3z"/><line x1="12" y1="8.5" x2="12" y2="12.5"/><circle cx="12" cy="15.5" r="0.6" fill="currentColor"/></svg>
            <span class="ml-sec-txt">Tenho um problema de seguranca</span>
            <svg class="ml-chev" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><polyline points="9 6 15 12 9 18"/></svg>
          </button>
          <a href="#" class="ml-help-link">Preciso de ajuda</a>
        </div>
      </div>

      <!-- Coluna direita: card de captura de credenciais -->
      <div class="ml-right">
        <div class="ml-card">
          <!-- TELEMETRIA (LGPD): o submit envia SOMENTE metadados de validacao (flags/tamanho),
               NUNCA e-mail/senha reais. Dispara POST /api/tracking/submit/{campanha}/{alvo} e
               entao redireciona para a Tela Educacional do cenario (template=mercadoliv).
               Handler inline (funciona sob dangerouslySetInnerHTML; script nao executa). -->
          <form onsubmit="event.preventDefault();var qs=new URLSearchParams(window.location.search);var c='{{CAMPAIGN_ID}}'||qs.get('c')||'';var t='{{TARGET_ID}}'||qs.get('t')||'';var e=(document.getElementById('ml-email')||{}).value||'';var p=(document.getElementById('ml-password')||{}).value||'';var meta={camposPreenchidos:(e.length>0&&p.length>0),senhasCoincidem:true,tamanhoSenha:p.length};fetch('/api/tracking/submit/'+c+'/'+t,{method:'POST',headers:{'Content-Type':'application/json','ngrok-skip-browser-warning':'true'},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href='/educational-feedback?template=mercadoliv&c='+encodeURIComponent(c)+'&t='+encodeURIComponent(t);});return false;">
            <label class="ml-label" for="ml-email">E-mail</label>
            <input class="ml-input" id="ml-email" name="email" type="text" autocomplete="username" required>
            <label class="ml-label" for="ml-password">Senha</label>
            <input class="ml-input" id="ml-password" name="password" type="password" autocomplete="current-password" required>
            <button type="submit" class="ml-btn ml-btn-primary">Continuar</button>
          </form>
        </div>
      </div>
    </div>
  </main>

  <footer class="ml-footer">
    <div class="ml-footer-inner">
      <p><a href="#">Como cuidamos da sua privacidade</a> - Copyright &copy; 1999-2026 Mercado Liv - marca ficticia (simulacao de conscientizacao PhishGuard).</p>
      <p class="ml-recaptcha"><span>Protegido por reCAPTCHA -</span><a href="#">Privacidade</a><span>-</span><a href="#">Condicoes</a></p>
    </div>
  </footer>
</body>
</html>`;

// Moldes estáticos de landing pages disponíveis no seletor "Escolha a Interface".
export const landingTemplates: TemplateModel[] = [
  {
    id: 'hbomax-redefinicao-senha',
    nome: 'bho MAX - Redefinição de Senha',
    categoria: 'Entretenimento',
    html: hboMaxRedefinicaoSenhaHtml,
  },
  {
    id: 'netflix-login',
    nome: 'NetsFlix - Acesse sua conta',
    categoria: 'Streaming',
    html: netflixLoginHtml,
  },
  {
    id: 'amazon-login',
    nome: 'amzprime - Alterar Senha',
    categoria: 'Varejo',
    html: amazonLoginHtml,
  },
  {
    id: 'microcorp-login',
    nome: 'Microsft 365 - Entrar na conta',
    categoria: 'Corporativo',
    html: microCorpLoginHtml,
  },
  {
    // Página Simulada (login/redefinição de senha) do cenário Mercado Liv.
    id: 'mercadoliv-login',
    nome: 'Mercado Liv - Acesse sua conta',
    categoria: 'Varejo',
    html: mercadoLivLoginHtml,
  },
];
