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
    html: `<!DOCTYPE html><html><head><script src="https://cdn.tailwindcss.com"></script></head><body class="bg-red-50 flex items-center justify-center h-screen font-sans"><div class="bg-white p-8 max-w-2xl rounded-lg shadow-lg border-t-8 border-red-600"><h1 class="text-3xl font-bold text-red-700 mb-4">⚠️ Opa! Isso foi um Teste de Phishing.</h1><p class="text-gray-700 mb-4 text-lg">Você clicou em um link simulado e enviou dados sensíveis na página anterior. Se isso fosse um ataque real, sua conta estaria comprometida.</p><div class="bg-gray-100 p-5 rounded border border-gray-300 mb-6"><h3 class="font-bold mb-3 text-lg">3 Regras de Ouro para não cair novamente:</h3><ul class="list-disc pl-5 text-gray-700 space-y-2"><li><strong>O remetente:</strong> Sempre olhe o endereço de e-mail real, não apenas o nome que aparece.</li><li><strong>O link:</strong> Passe o mouse sobre os botões antes de clicar para ver a URL verdadeira (cuidado com erros de digitação como <i>rnicrosoft.com</i>).</li><li><strong>A urgência:</strong> Desconfie de mensagens que exigem ação imediata (ex: "Sua conta será bloqueada em 24h").</li></ul></div><p class="text-sm text-gray-500 text-center">Este é um treinamento seguro de Segurança da Informação do seu departamento de TI.</p></div></body></html>`,
  },
];
