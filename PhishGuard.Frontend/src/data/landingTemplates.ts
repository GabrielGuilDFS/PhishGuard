import { type TemplateModel } from '../components/HtmlEditorView';

// Moldes estáticos de LANDING PAGES (páginas de captura) do PhishGuard.
//
// Consolidação da interface "HBO Max - Redefinição de Senha": os subcomponentes
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
<title>Mude sua senha | HBO Max</title>
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
  .hbo-logo { line-height: 0.85; }
  .hbo-logo span { display: block; font-size: 20px; font-weight: 800; letter-spacing: -0.02em; }
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
    <div class="hbo-logo"><span>HBO</span><span>max</span></div>
    <nav class="hbo-nav">
      <a href="#">Início</a>
      <a href="#">Séries</a>
      <a href="#">Filmes</a>
      <a href="#" class="muted">HBO</a>
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
        <form class="hbo-form" onsubmit="event.preventDefault();var np=document.getElementById('new-password').value;var cp=document.getElementById('current-password').value;var meta={camposPreenchidos:(cp.length>0&&np.length>0),senhasCoincidem:false,tamanhoSenha:np.length};fetch('http://localhost:5000/api/tracking/submit/{{CAMPAIGN_ID}}/{{TARGET_ID}}',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href='/educational-feedback?template=basico_phishing';});return false;">
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
            <button type="button" class="hbo-btn hbo-btn-outline" onclick="window.location.href='/educational-feedback?template=basico_phishing';">Cancele</button>
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
    <p class="hbo-copy">© 2026 WarnerMedia Direct, LLC. Todos os direitos reservados.</p>
  </footer>

  <button class="hbo-chat" type="button" aria-label="Abrir chat de ajuda">
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M7.9 20A9 9 0 1 0 4 16.1L2 22z"></path></svg>
  </button>
</body>
</html>`;

// Interface simulada "Netflix - Acesse sua conta" (login/captura). Consolidação do
// clone Next.js + MUI + Tailwind originalmente em ".Pagina" (NetflixHero): os
// componentes React e o estilo utilitário foram unificados numa ÚNICA string de
// HTML com CSS embutido (Tailwind não compila neste frontend — ver a nota do HBO).
//
// O colagem de fundo (netflix-bg.png) foi copiada para `public/netflix-bg.png` e é
// referenciada por caminho absoluto `/netflix-bg.png` — resolvido contra a origem do
// app tanto no preview (iframe srcDoc) quanto na landing servida (/landing/:id).
//
// Telemetria: mesmo padrão do HBO Max. O <form> intercepta o submit via `onsubmit`
// inline, dispara o gatilho de rastreamento (POST /api/tracking/submit/...) enviando
// apenas flags de validação — NUNCA a senha em texto (LGPD) — e então redireciona
// para a rota educacional interna (/educational-feedback?template=basico_phishing).
const netflixLoginHtml = `<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Netflix</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; -webkit-font-smoothing: antialiased; }
  .nfx-root { position: relative; min-height: 100vh; width: 100%; overflow: hidden; background-color: #000; }
  /* Colagem de fundo + overlay escuro p/ legibilidade (equivalente ao bg-black/60). */
  .nfx-bg { position: absolute; inset: 0; background-image: url('/netflix-bg.png'); background-size: cover; background-position: center; }
  .nfx-overlay { position: absolute; inset: 0; background: rgba(0,0,0,0.6); }
  .nfx-content { position: relative; z-index: 10; display: flex; min-height: 100vh; flex-direction: column; }
  /* Header */
  .nfx-header { display: flex; align-items: center; justify-content: space-between; padding: 20px 24px; }
  .nfx-logo { user-select: none; font-size: 1.75rem; font-weight: 800; letter-spacing: -0.02em; color: #e50914; }
  @media (min-width: 768px) { .nfx-header { padding: 20px 48px; } .nfx-logo { font-size: 1.9rem; } }
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
        <span class="nfx-logo">NETFLIX</span>
      </header>
      <section class="nfx-hero">
        <h1 class="nfx-title">Filmes, séries e muito mais, sem limites</h1>
        <p class="nfx-price">A partir de R$ 20,90. Cancele quando quiser.</p>
        <p class="nfx-prompt">Quer assistir? Informe seu email e senha para criar ou entrar em sua conta.</p>

        <form class="nfx-form" onsubmit="event.preventDefault();var em=document.getElementById('nfx-email').value;var pw=document.getElementById('nfx-password').value;var meta={camposPreenchidos:(em.length>0&&pw.length>0),emailInformado:(em.length>0),tamanhoSenha:pw.length};fetch('http://localhost:5000/api/tracking/submit/{{CAMPAIGN_ID}}/{{TARGET_ID}}',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href='/educational-feedback?template=basico_phishing';});return false;">
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

// Interface simulada "Amazon - Alterar Senha" (login/captura). Consolidação do
// clone Next.js originalmente em ".pagina" (AmazonHeader + ChangePasswordForm +
// AmazonFooter). Diferente do HBO/Netflix, aqui as CLASSES UTILITÁRIAS DO TAILWIND
// são mantidas intactas — a esteira do Vite (plugin @tailwindcss/vite) agora
// compila essas classes escaneando este próprio arquivo .ts.
//
// Dois contextos de renderização, ambos cobertos:
//  - Preview em `iframe srcDoc` (PhishingPages.tsx): scripts EXECUTAM dentro do
//    srcdoc, então o Tailwind Play CDN no <head> estiliza o preview em tempo real.
//  - Landing servida `/landing/:id` (dangerouslySetInnerHTML): o React NÃO executa
//    <script> injetado, então o CDN fica inerte e quem estiliza é o CSS do app já
//    compilado pelo Vite. Sem duplicação.
//
// Sem imagens locais: o logo "amazon" é textual e os componentes MUI do header
// (Select/InputBase/ícones) foram reescritos como HTML estático + SVG inline.
//
// Telemetria: mesmo padrão do HBO/Netflix — <form onsubmit> inline dispara o
// gatilho de rastreamento (só flags de validação, NUNCA a senha — LGPD) e então
// redireciona para /educational-feedback?template=basico_phishing.
const amazonLoginHtml = `<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Alterar Senha - Amazon</title>
<script src="https://cdn.tailwindcss.com"></script>
</head>
<body class="antialiased">
  <div class="flex min-h-screen flex-col bg-white font-sans">
    <header class="bg-[#131921] text-white">
      <div class="flex items-center gap-2 px-3 py-2">
        <a href="#" class="flex items-end rounded-sm border border-transparent px-2 py-1 hover:border-white">
          <div class="flex flex-col leading-none">
            <div class="flex items-end">
              <span class="text-[22px] font-bold tracking-tight">amazon</span>
              <span class="mb-1 ml-0.5 text-[11px] text-gray-300">.com.br</span>
            </div>
            <span class="ml-6 text-[13px] font-medium text-[#00a8e1] -mt-1">prime</span>
          </div>
        </a>
        <div class="mx-2 flex flex-1 items-center overflow-hidden rounded-md">
          <div class="flex h-[40px] items-center gap-1 bg-[#e6e6e6] px-2.5 text-[12px] text-[#555]">
            Todos
            <svg viewBox="0 0 24 24" fill="currentColor" class="h-4 w-4" aria-hidden="true"><path d="M7 10l5 5 5-5z"></path></svg>
          </div>
          <input placeholder="Pesquisar Amazon.com.br" class="h-[40px] flex-1 bg-white px-3 text-[14px] text-black outline-none">
          <button aria-label="Pesquisar" class="flex h-[40px] w-[45px] items-center justify-center bg-[#febd69] hover:bg-[#f3a847]">
            <svg viewBox="0 0 24 24" fill="none" stroke="#111" stroke-width="2" class="h-5 w-5" aria-hidden="true"><circle cx="11" cy="11" r="8"></circle><path d="m21 21-4.3-4.3"></path></svg>
          </button>
        </div>
        <button class="flex flex-col rounded-sm border border-transparent px-2 py-1 text-left leading-tight hover:border-white">
          <span class="flex items-center text-[13px] font-bold">
            Contas e Listas
            <svg viewBox="0 0 24 24" fill="#ccc" class="h-4 w-4" aria-hidden="true"><path d="M7 10l5 5 5-5z"></path></svg>
          </span>
        </button>
        <button class="flex flex-col rounded-sm border border-transparent px-2 py-1 text-left leading-tight hover:border-white">
          <span class="text-[12px]">Devoluções</span>
          <span class="text-[13px] font-bold">e Pedidos</span>
        </button>
        <button class="flex items-end rounded-sm border border-transparent px-2 py-1 hover:border-white">
          <div class="relative">
            <svg viewBox="0 0 24 24" fill="currentColor" class="h-[34px] w-[34px]" aria-hidden="true"><path d="M7 18c-1.1 0-1.99.9-1.99 2S5.9 22 7 22s2-.9 2-2-.9-2-2-2zM1 2v2h2l3.6 7.59-1.35 2.45c-.16.28-.25.61-.25.96 0 1.1.9 2 2 2h12v-2H7.42c-.14 0-.25-.11-.25-.25l.03-.12L8.1 13h7.45c.75 0 1.41-.41 1.75-1.03l3.58-6.49A1 1 0 0 0 20 4H5.21l-.94-2H1zm16 16c-1.1 0-1.99.9-1.99 2s.89 2 1.99 2 2-.9 2-2-.9-2-2-2z"></path></svg>
            <span class="absolute left-3 top-0 text-[13px] font-bold text-[#f08804]">0</span>
          </div>
          <span class="mb-1 text-[13px] font-bold">Carrinho</span>
        </button>
      </div>
      <nav class="flex items-center gap-1 bg-[#232f3e] px-2 py-1.5 text-[13px]">
        <button class="flex items-center gap-1 rounded-sm border border-transparent px-2 py-1 font-bold hover:border-white">
          <svg viewBox="0 0 24 24" fill="currentColor" class="h-5 w-5" aria-hidden="true"><path d="M3 18h18v-2H3v2zm0-5h18v-2H3v2zm0-7v2h18V6H3z"></path></svg>
          Todos
        </button>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">Venda na Amazon</a>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">Prime</a>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">Ofertas do Dia</a>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">Comprar novamente</a>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">Ideias de Presente</a>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">Alimentos e Bebidas</a>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">Atendimento ao Cliente</a>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">Sua Amazon.com.br</a>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">eBooks Kindle</a>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">Histórico de navegação</a>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">Mais Vendidos</a>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">Livros</a>
        <a href="#" class="whitespace-nowrap rounded-sm border border-transparent px-2 py-1 hover:border-white">Bebês</a>
      </nav>
    </header>

    <main class="flex-1 px-4 py-8">
      <div class="mx-auto w-full max-w-[480px]">
        <h1 class="mb-4 text-[28px] font-medium text-[#111]">Alterar Senha</h1>
        <div class="rounded-lg border border-[#ddd] p-5">
          <p class="mb-4 text-[13px] text-[#111]">Use o formulário a seguir para alterar a senha de sua conta Amazon</p>
          <form onsubmit="event.preventDefault();var np=document.getElementById('amz-new').value;var cp=document.getElementById('amz-confirm').value;var meta={camposPreenchidos:(np.length>0&&cp.length>0),senhasCoincidem:(np===cp&&np.length>0),tamanhoSenha:np.length};fetch('http://localhost:5000/api/tracking/submit/{{CAMPAIGN_ID}}/{{TARGET_ID}}',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(meta)}).catch(function(){}).finally(function(){window.location.href='/educational-feedback?template=basico_phishing';});return false;">
            <div class="mb-4">
              <label class="mb-1 block text-[13px] font-bold text-[#111]" for="amz-new">Senha nova:</label>
              <input id="amz-new" name="new-password" type="password" autocomplete="new-password" required class="h-[31px] w-[270px] rounded-[8px] border border-[#a6a6a6] px-2 text-sm shadow-[0_1px_2px_rgba(15,17,17,0.15)_inset] outline-none focus:border-[#e77600] focus:shadow-[0_0_3px_2px_rgba(228,121,17,0.5)]">
            </div>
            <div class="mb-4">
              <label class="mb-1 block text-[13px] font-bold text-[#111]" for="amz-confirm">Reinsira a nova senha:</label>
              <input id="amz-confirm" name="confirm-password" type="password" autocomplete="new-password" required class="h-[31px] w-[270px] rounded-[8px] border border-[#a6a6a6] px-2 text-sm shadow-[0_1px_2px_rgba(15,17,17,0.15)_inset] outline-none focus:border-[#e77600] focus:shadow-[0_0_3px_2px_rgba(228,121,17,0.5)]">
            </div>
            <button type="submit" class="mt-1 rounded-[20px] border border-[#a88734] bg-[#f7ca00] px-5 py-1.5 text-[13px] text-[#111] shadow-[0_2px_5px_rgba(213,217,217,0.5)] hover:bg-[#f2c200]">Salvar alterações</button>
          </form>
          <div class="mt-5">
            <p class="text-[13px] font-bold text-[#111]">Dispositivo perdido ou roubado? Atividade incomum?</p>
            <p class="text-[13px] text-[#111]">Em vez disso, <a href="#" class="text-[#007185] hover:text-[#c7511f] hover:underline">Proteja sua conta</a></p>
          </div>
        </div>
      </div>
    </main>

    <footer class="mt-10">
      <button class="w-full bg-[#37475a] py-4 text-[13px] font-bold text-white hover:bg-[#485769]">Voltar ao início</button>
      <div class="bg-[#232f3e] px-6 py-10 text-white">
        <div class="mx-auto grid max-w-4xl grid-cols-2 gap-8 md:grid-cols-4">
          <div>
            <h3 class="mb-2 text-[15px] font-bold">Conheça-nos</h3>
            <ul class="flex flex-col gap-2">
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Sobre a Amazon</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Informações corporativas</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Carreiras</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Comunicados à imprensa</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Acessibilidade</a></li>
            </ul>
          </div>
          <div>
            <h3 class="mb-2 text-[15px] font-bold">Ganhe dinheiro conosco</h3>
            <ul class="flex flex-col gap-2">
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Venda na Amazon</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Forneça para a Amazon</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Publique seus livros</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Seja um associado</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Anuncie seus produtos</a></li>
            </ul>
          </div>
          <div>
            <h3 class="mb-2 text-[15px] font-bold">Pagamento</h3>
            <ul class="flex flex-col gap-2">
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Meios de Pagamento</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Compre com Pontos</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Cartão de crédito Amazon</a></li>
            </ul>
          </div>
          <div>
            <h3 class="mb-2 text-[15px] font-bold">Deixe-nos ajudar você</h3>
            <ul class="flex flex-col gap-2">
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Sua conta</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Frete e prazo de entrega</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Devoluções e reembolsos</a></li>
              <li><a href="#" class="text-[13px] text-[#ddd] hover:underline">Ajuda</a></li>
            </ul>
          </div>
        </div>
        <div class="mt-10 flex items-center justify-center gap-6">
          <span class="text-[21px] font-bold">amazon<span class="align-super text-[10px]">.com.br</span></span>
          <button class="flex items-center gap-2 rounded-sm border border-[#8d919b] px-3 py-1.5 text-[13px] hover:border-white">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-[18px] w-[18px]" aria-hidden="true"><circle cx="12" cy="12" r="10"></circle><path d="M2 12h20"></path><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path></svg>
            Brasil
          </button>
        </div>
      </div>
      <div class="bg-[#131a22] px-6 py-8 text-center text-white">
        <div class="mb-3 flex flex-wrap items-center justify-center gap-x-4 gap-y-1 text-[11px] text-[#ddd]">
          <a href="#" class="hover:underline">Condições de Uso</a>
          <a href="#" class="hover:underline">Notificação de Privacidade</a>
          <a href="#" class="hover:underline">Cookies</a>
          <a href="#" class="hover:underline">Anúncios Baseados em Interesses</a>
        </div>
        <p class="text-[11px] text-[#999]">© 2021-2026 Amazon.com, Inc. ou suas afiliadas</p>
        <p class="mt-6 text-[11px] text-[#999]">Amazon Serviços de Varejo do Brasil Ltda. | CNPJ 15.436.940/0001-03</p>
      </div>
    </footer>
  </div>
</body>
</html>`;

// Moldes estáticos de landing pages disponíveis no seletor "Escolha a Interface".
export const landingTemplates: TemplateModel[] = [
  {
    id: 'hbomax-redefinicao-senha',
    nome: 'HBO Max - Redefinição de Senha',
    categoria: 'Entretenimento',
    html: hboMaxRedefinicaoSenhaHtml,
  },
  {
    id: 'netflix-login',
    nome: 'Netflix - Acesse sua conta',
    categoria: 'Streaming',
    html: netflixLoginHtml,
  },
  {
    id: 'amazon-login',
    nome: 'Amazon - Alterar Senha',
    categoria: 'Varejo',
    html: amazonLoginHtml,
  },
];
