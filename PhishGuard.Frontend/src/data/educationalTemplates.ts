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
];
