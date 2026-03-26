import HtmlEditorView, { type TemplateModel } from '../components/HtmlEditorView';

const educationalTemplates: TemplateModel[] = [
  {
    id: 'basico_phishing',
    nome: 'Treinamento Básico: O que é Phishing?',
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
</html>`
  }
];

export default function EducationalPages() {
  return (
    <HtmlEditorView
      title="Páginas Educacionais (Treinamentos)"
      apiEndpoint="http://localhost:5000/api/EducationalPages"
      emptyMessage="Nenhuma página educacional cadastrada. Crie seu primeiro treinamento!"
      defaultTemplates={educationalTemplates}
    />
  );
}
