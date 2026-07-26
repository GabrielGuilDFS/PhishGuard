import { type TemplateModel } from './templateTypes';

// Catálogo OFICIAL e ESTÁTICO das Páginas Educativas (abordagens pedagógicas de
// conscientização exibidas ao alvo ao final do fluxo de simulação).
//
// Padronização (refatoração de Cenários de Simulação): os dois moldes que antes
// viviam espalhados (EducationalPages.tsx e App.tsx) foram consolidados aqui.
// Não há mais edição de HTML bruto pelo usuário — o administrador apenas escolhe
// um destes moldes fixos. A rota /educational-feedback resolve o molde por id.
export const educationalTemplates: TemplateModel[] = [
  {
    id: 'basico_phishing',
    nome: 'Treinamento Básico: O que é Phishing?',
    categoria: 'Conscientização',
    html: `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>Treinamento de Segurança</title>
  <style>
    body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #fce4ec; color: #333; margin: 0; padding: 40px; }
    .card { background: white; padding: 40px; border-radius: 8px; max-width: 600px; margin: 0 auto; box-shadow: 0 4px 12px rgba(0,0,0,0.1); border-top: 8px solid #c2185b; }
    h1 { color: #c2185b; margin-top: 0; }
    h3 { color: #d81b60; }
    .info { background: #f8bbd0; padding: 15px; border-radius: 4px; margin: 20px 0; border-left: 4px solid #c2185b; }
    p { line-height: 1.6; }
  </style>
</head>
<body>
  <div class="card">
    <h1>Ops! Você clicou em um link simulado de Phishing.</h1>
    <p>Não se preocupe, isso foi apenas um teste de segurança realizado pela nossa equipe para ajudá-lo a identificar ameaças reais no futuro.</p>

    <div class="info">
      <strong>O que é Phishing?</strong><br>
      É uma técnica usada por criminosos para enganar pessoas e roubar informações sensíveis, como senhas e dados de cartão de crédito.
    </div>

    <h3>Como identificar e-mails suspeitos:</h3>
    <ul>
      <li>Verifique o endereço de e-mail do remetente com atenção.</li>
      <li>Desconfie de senso de urgência ou ameaças.</li>
      <li>Não clique em links nem baixe anexos desconhecidos.</li>
      <li>Alerte a equipe de TI em caso de dúvida.</li>
    </ul>

    <p style="text-align: center; margin-top: 30px; font-weight: bold;">
      Continue seguro e lembre-se: na dúvida, não clique!
    </p>
  </div>
</body>
</html>`,
  },
  {
    id: 'treinamento_padrao',
    nome: 'Treinamento de Phishing Padrão (Tailwind)',
    categoria: 'Conscientização',
    // AUTO-CONTIDO (CSS embutido, sem CDN JS) — mesmo motivo do amazon-login: o preview
    // (iframe com sandbox sem allow-scripts) e a página servida (dangerouslySetInnerHTML,
    // que ignora <script>) bloqueiam o Tailwind Play CDN, deixando o template sem estilo.
    html: `<!DOCTYPE html><html lang="pt-BR"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><style>*{box-sizing:border-box;margin:0;padding:0}body{min-height:100vh;display:flex;align-items:center;justify-content:center;background:#fef2f2;font-family:system-ui,-apple-system,'Segoe UI',Roboto,Arial,sans-serif;padding:24px;color:#374151}a{color:inherit;text-decoration:none}.edu-card{background:#fff;width:100%;max-width:42rem;padding:32px;border-radius:8px;box-shadow:0 10px 15px -3px rgba(0,0,0,.1),0 4px 6px -4px rgba(0,0,0,.1);border-top:8px solid #dc2626}.edu-card h1{font-size:1.875rem;line-height:1.2;font-weight:700;color:#b91c1c;margin-bottom:16px}.edu-lead{font-size:1.125rem;color:#374151;margin-bottom:16px}.edu-box{background:#f3f4f6;border:1px solid #d1d5db;border-radius:6px;padding:20px;margin-bottom:24px}.edu-box h3{font-size:1.125rem;font-weight:700;margin-bottom:12px;color:#111827}.edu-box ul{list-style:disc;padding-left:20px;display:flex;flex-direction:column;gap:8px;color:#374151}.edu-note{font-size:.875rem;color:#6b7280;text-align:center}</style></head><body><div class="edu-card"><h1>⚠️ Opa! Isso foi um Teste de Phishing.</h1><p class="edu-lead">Você clicou em um link simulado e enviou dados sensíveis na página anterior. Se isso fosse um ataque real, sua conta estaria comprometida.</p><div class="edu-box"><h3>3 Regras de Ouro para não cair novamente:</h3><ul><li><strong>O remetente:</strong> Sempre olhe o endereço de e-mail real, não apenas o nome que aparece.</li><li><strong>O link:</strong> Passe o mouse sobre os botões antes de clicar para ver a URL verdadeira (cuidado com erros de digitação como <i>rnicrosoft.com</i>).</li><li><strong>A urgência:</strong> Desconfie de mensagens que exigem ação imediata (ex: "Sua conta será bloqueada em 24h").</li></ul></div><p class="edu-note">Este é um treinamento seguro de Segurança da Informação do seu departamento de TI.</p></div></body></html>`,
  },
  {
    // PLACEHOLDER TEMPORÁRIO — Tela Educacional do cenário "Mercado Liv". O treinamento
    // interativo (Just-in-Time) ainda não foi finalizado; enquanto isso, exibimos um card
    // "clean" com a mensagem de "Página em desenvolvimento" para não quebrar o fluxo
    // (rota /educational-feedback e preview da Biblioteca de Modelos). Paleta da marca
    // (amarelo #FEE501 / azul #2E347E). Ao finalizar, substituir por uma entrada em
    // feedbackTrainings (treinamento interativo) e apontar o cenário para o novo id.
    id: 'mercadoliv-em-desenvolvimento',
    nome: 'Mercado Liv — Treinamento (em desenvolvimento)',
    categoria: 'Conscientização',
    html: `<!DOCTYPE html><html lang="pt-BR"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Mercado Liv — Em desenvolvimento</title><style>*{box-sizing:border-box;margin:0;padding:0}body{min-height:100vh;display:flex;align-items:center;justify-content:center;background:#f4f5fa;font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;padding:24px;color:#1e2244}.card{width:100%;max-width:480px;background:#fff;border-radius:16px;box-shadow:0 12px 40px rgba(46,52,126,.12);overflow:hidden;text-align:center}.card-top{background:#FEE501;padding:18px 22px;display:flex;align-items:center;justify-content:center;gap:12px}.brand{font-size:22px;font-weight:800;letter-spacing:-.5px;color:#2e347e}.brand-logo{display:block;height:34px;width:auto}.card-body{padding:36px 32px 40px}.icon{width:64px;height:64px;margin:0 auto 20px;border-radius:50%;background:#eef0fb;color:#2E347E;display:flex;align-items:center;justify-content:center;font-size:30px}h1{font-size:20px;font-weight:700;color:#2E347E;margin-bottom:10px}p{font-size:14px;line-height:1.6;color:#55597a}.tag{display:inline-block;margin-top:22px;padding:7px 16px;border-radius:999px;background:#fff8cc;color:#7a6a00;font-size:12px;font-weight:700;border:1px solid #f2e28a}</style></head><body><div class="card"><div class="card-top"><span class="brand">Mercado Liv</span><img class="brand-logo" src="data:image/png;base64,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" alt="Mercado Liv" /></div><div class="card-body"><div class="icon">🚧</div><h1>Página em desenvolvimento</h1><p>Funcionalidade não finalizada. A tela educacional deste cenário ainda está sendo construída.</p><span class="tag">Em breve</span></div></div></body></html>`,
  },
];
