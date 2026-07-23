import { templatesPredefinidos } from './predefinedTemplates';
import { landingTemplates } from './landingTemplates';
import { formatarExpiracaoLink, formatarDataAcessoBRT } from '../utils/emailExpiration';

// ============================================================================
// Configuração das Telas Educacionais de Feedback (Just-in-Time Training)
// ============================================================================
//
// Objeto de configuração LIMPO e SEPARADO da renderização: o componente
// `components/FeedbackTraining.tsx` é 100% genérico e é dirigido por um destes
// registros. Para adicionar o treinamento de uma nova isca (bho MAX, Mercado
// Liv, NetsFlix), basta acrescentar uma entrada em `feedbackTrainings` — sem
// tocar no componente.
//
// A rota /educational-feedback resolve a config pelo `?template=<id>`; se não
// houver config, cai no catálogo estático legado (educationalTemplates.ts).

/** Um ponto de atenção numerado sobreposto ao mockup do e-mail ou da landing. */
export interface FeedbackHotspot {
  /** Número exibido no badge; casa 1:1 com o `numero` do card explicativo. */
  numero: number;
  /** Posição do badge, em % relativo à área do mockup (0–100). */
  xPct: number;
  yPct: number;
  /** Texto curto no tooltip/aria do badge. */
  dica: string;
}

/** "Cromo" (moldura) desenhado acima do iframe para dar contexto realista. */
export type FeedbackMockupChrome =
  | { tipo: 'email'; remetenteNome: string; remetenteEmail: string; assunto: string }
  | { tipo: 'navegador'; url: string; seguro: boolean };

/** Um painel visual da isca (e-mail OU landing page) com seus hotspots. */
export interface FeedbackMockup {
  id: 'email' | 'landing';
  titulo: string;
  /** Moldura de contexto (cliente de e-mail ou barra do navegador). */
  chrome: FeedbackMockupChrome;
  /** HTML da isca renderizado num iframe sandbox (somente leitura). */
  html: string;
  /** Largura lógica do documento, para escalar o iframe sem quebrar o layout. */
  larguraLogica: number;
  /** Altura visível do mockup (px). */
  altura: number;
  hotspots: FeedbackHotspot[];
}

/** Card explicativo correspondente a um número de hotspot. */
export interface FeedbackCard {
  numero: number;
  /** Emoji/ícone representativo do indicador de risco. */
  icone: string;
  titulo: string;
  /** Explicação curta e direta em bullet points. */
  pontos: string[];
}

/** Configuração completa de uma Tela Educacional de Feedback. */
export interface FeedbackTrainingConfig {
  id: string;
  /** Nome da marca simulada, exibido no cabeçalho ("Simulação: amzprime"). */
  marca: string;
  header: {
    badge: string;
    titulo: string;
    mensagem: string;
  };
  mockups: FeedbackMockup[];
  cards: FeedbackCard[];
  conclusao: {
    label: string;
    /** Rota interna de destino após concluir (padrão: landing do sistema). */
    redirecionarPara: string;
  };
}

// --- Helpers de resolução do HTML bruto das iscas -------------------------
// Reutiliza os MESMOS HTMLs já mantidos nos catálogos (e-mail e landing), sem
// duplicar markup. Placeholders de disparo/telemetria são neutralizados: no
// mockup educativo nada dispara rastreamento nem navega para lugar nenhum.
function resolverEmailHtml(templateId: string): string {
  const t = templatesPredefinidos.find((x) => x.id === templateId);
  return neutralizarPlaceholders(t?.corpoHtml ?? '');
}

function resolverLandingHtml(landingId: string): string {
  const l = landingTemplates.find((x) => x.id === landingId);
  return neutralizarPlaceholders(l?.html ?? '');
}

function neutralizarPlaceholders(html: string): string {
  return html
    .replace(/{{LINK_PHISHING}}/g, '#')
    .replace(/{{CAMPAIGN_ID}}/g, 'demo')
    .replace(/{{TARGET_ID}}/g, 'demo')
    .replace(/{{NOME}}/g, 'Usuário')
    .replaceAll('{{DATA_EXPIRACAO}}', formatarExpiracaoLink())
    .replaceAll('{{DATA_ACESSO}}', formatarDataAcessoBRT());
}

// ============================================================================
// Registro de treinamentos por isca
// ============================================================================
export const feedbackTrainings: Record<string, FeedbackTrainingConfig> = {
  amzprime: {
    id: 'amzprime',
    marca: 'amzprime',
    header: {
      badge: 'Atenção: Esta foi uma simulação de treinamento de segurança do PhishGuard.',
      titulo: 'Você interagiu com um e-mail de phishing simulado.',
      mensagem:
        'Não se preocupe: nenhum dado real foi comprometido — apenas o clique foi ' +
        'registrado. Use os pontos destacados abaixo para aprender a identificar esse ' +
        'tipo de ataque no futuro.',
    },
    mockups: [
      {
        id: 'email',
        titulo: 'O e-mail que você recebeu',
        chrome: {
          tipo: 'email',
          remetenteNome: 'amzprime',
          remetenteEmail: 'account-security@amzprime.com',
          assunto: 'Alerta de segurança: atividade incomum na sua conta',
        },
        html: resolverEmailHtml('amazon-notificacao-seguranca'),
        larguraLogica: 600,
        altura: 460,
        hotspots: [
          { numero: 1, xPct: 26, yPct: 12, dica: 'Domínio do remetente e saudação genérica' },
          { numero: 2, xPct: 22, yPct: 55, dica: 'Gatilho de urgência no botão' },
          { numero: 6, xPct: 50, yPct: 92, dica: 'CNPJ e logo — verifique o oficial' },
        ],
      },
      {
        id: 'landing',
        titulo: 'A página falsa de alteração de senha',
        chrome: {
          tipo: 'navegador',
          url: 'amzprime-account-security.help/alterar-senha',
          seguro: false,
        },
        html: resolverLandingHtml('amazon-login'),
        larguraLogica: 1024,
        altura: 460,
        hotspots: [
          { numero: 3, xPct: 30, yPct: 3, dica: 'URL na barra de endereço' },
          { numero: 4, xPct: 82, yPct: 14, dica: 'Sem 2FA e links de cabeçalho inativos' },
          { numero: 5, xPct: 40, yPct: 55, dica: 'Pede nova senha sem cadastro prévio' },
        ],
      },
    ],
    cards: [
      {
        numero: 1,
        icone: '📧',
        titulo: 'Remetente e saudação suspeitos',
        pontos: [
          'O domínio "amzprime.com" imita uma marca real, mas não é o domínio oficial dela.',
          'A saudação é genérica ("Olá,") — empresas legítimas costumam usar seu nome.',
          'Não há foto de perfil/avatar verificado do remetente no cliente de e-mail.',
        ],
      },
      {
        numero: 2,
        icone: '⏰',
        titulo: 'Gatilho de urgência',
        pontos: [
          'O botão "Alterar sua senha" cria pressão para agir sem pensar.',
          'Ameaças de "acesso incomum" exploram o medo para acelerar o clique.',
          'Na dúvida, acesse o site oficial digitando a URL você mesmo — nunca pelo botão.',
        ],
      },
      {
        numero: 3,
        icone: '🔗',
        titulo: 'URL da página falsa',
        pontos: [
          'A barra de endereço mostra um domínio estranho, não o site oficial.',
          'Subdomínios e sufixos como ".help" tentam parecer legítimos.',
          'Confira sempre o domínio completo antes de digitar qualquer senha.',
        ],
      },
      {
        numero: 4,
        icone: '🔒',
        titulo: 'Sem 2FA e links inativos',
        pontos: [
          'A página não oferece verificação em dois fatores (2FA).',
          'Os links do cabeçalho e rodapé não levam a lugar nenhum ("#").',
          'Sites reais têm navegação funcional e camadas extras de segurança.',
        ],
      },
      {
        numero: 5,
        icone: '🤔',
        titulo: 'Pedido incoerente de senha',
        pontos: [
          'A página pede para "criar uma senha nova" sem você ter feito cadastro.',
          'Fluxos legítimos exigem confirmar a senha atual ou um link enviado por e-mail.',
          'Incoerência no fluxo é um forte sinal de fraude.',
        ],
      },
      {
        numero: 6,
        icone: '🏢',
        titulo: 'CNPJ e logotipo',
        pontos: [
          'O CNPJ do rodapé é inventado — compare com o registro oficial da empresa.',
          'A logo é uma imitação textual ("amzprime"), não a marca registrada real.',
          'Detalhes de identidade visual e dados cadastrais denunciam a falsificação.',
        ],
      },
    ],
    conclusao: {
      label: 'Concluir Treinamento',
      redirecionarPara: '/',
    },
  },

  netsflix: {
    id: 'netsflix',
    marca: 'NetsFlix',
    header: {
      badge: 'Atenção: Esta foi uma simulação de treinamento de segurança do PhishGuard.',
      titulo: 'Você interagiu com um e-mail de phishing simulado.',
      mensagem:
        'Não se preocupe: nenhum dado real foi comprometido — apenas o clique foi ' +
        'registrado. Use os pontos destacados abaixo para aprender a identificar esse ' +
        'tipo de ataque no futuro.',
    },
    mockups: [
      {
        id: 'email',
        titulo: 'O e-mail que você recebeu',
        chrome: {
          tipo: 'email',
          remetenteNome: 'NetsFlix',
          remetenteEmail: 'info@account.netsflix.com',
          assunto: 'Ação necessária: faltam dados da conta',
        },
        html: resolverEmailHtml('netflix-atualizacao-cobranca'),
        larguraLogica: 600,
        altura: 460,
        hotspots: [
          { numero: 2, xPct: 30, yPct: 16, dica: 'Saudação genérica e marca alterada' },
          { numero: 1, xPct: 60, yPct: 42, dica: 'Falsa exigência de autoridades' },
          { numero: 3, xPct: 28, yPct: 66, dica: 'Botão de ação com destino suspeito' },
        ],
      },
      {
        id: 'landing',
        titulo: 'A página falsa de login',
        chrome: {
          tipo: 'navegador',
          url: 'netsflix-conta-verificar.help/entrar',
          seguro: false,
        },
        html: resolverLandingHtml('netflix-login'),
        larguraLogica: 1024,
        altura: 460,
        hotspots: [
          { numero: 4, xPct: 50, yPct: 62, dica: 'Pede senha direto na tela de apresentação' },
        ],
      },
    ],
    cards: [
      {
        numero: 1,
        icone: '⚖️',
        titulo: 'Falsa Exigência Legal',
        pontos: [
          "Golpistas frequentemente citam 'exigências de autoridades' ou 'atualizações obrigatórias de CPF/CEP' para assustar a vítima e induzir uma ação rápida.",
        ],
      },
      {
        numero: 2,
        icone: '👋',
        titulo: 'Saudação Genérica e Alteração da Marca',
        pontos: [
          "O e-mail utiliza 'Olá, Cliente' em vez do seu nome.",
          "Além disso, observe a grafia alterada para 'NetsFlix' e o uso do ícone genérico 'N'.",
        ],
      },
      {
        numero: 3,
        icone: '🔗',
        titulo: 'Botão de Ação Suspeito',
        pontos: [
          "Passar o mouse sobre o botão 'Adicionar detalhes de cobrança' revela uma URL diferente do serviço oficial de streaming.",
        ],
      },
      {
        numero: 4,
        icone: '🔑',
        titulo: 'Solicitação Direta de Senha na Tela Inicial',
        pontos: [
          'A página falsa exige login e senha diretamente em uma tela de apresentação, sem fluxo seguro de autenticação ou verificação em duas etapas (2FA).',
        ],
      },
    ],
    conclusao: {
      label: 'Concluir Treinamento',
      redirecionarPara: '/',
    },
  },

  bhomax: {
    id: 'bhomax',
    marca: 'bho MAX',
    header: {
      badge: 'Atenção: Esta foi uma simulação de treinamento de segurança do PhishGuard.',
      titulo: 'Você interagiu com um e-mail de phishing simulado.',
      mensagem:
        'Não se preocupe: nenhum dado real foi comprometido — apenas o clique foi ' +
        'registrado. Use os pontos destacados abaixo para aprender a identificar esse ' +
        'tipo de ataque no futuro.',
    },
    mockups: [
      {
        id: 'email',
        titulo: 'O e-mail que você recebeu',
        chrome: {
          tipo: 'email',
          remetenteNome: 'bho MAX',
          remetenteEmail: 'no-reply@alerts.bhomax.com',
          assunto: 'Seu link para alteração de senha solicitado',
        },
        html: resolverEmailHtml('hbomax-redefinicao-senha'),
        larguraLogica: 600,
        altura: 460,
        hotspots: [
          { numero: 5, xPct: 26, yPct: 3, dica: 'Domínio do remetente (subdomínio "alerts.")' },
          { numero: 1, xPct: 30, yPct: 15, dica: 'Nome da marca invertido' },
          { numero: 2, xPct: 55, yPct: 27, dica: 'Horário em fuso EST + prazo curto' },
        ],
      },
      {
        id: 'landing',
        titulo: 'A página falsa de redefinição de senha',
        chrome: {
          tipo: 'navegador',
          url: 'bhomax-conta-seguranca.help/mudar-senha',
          seguro: false,
        },
        html: resolverLandingHtml('hbomax-redefinicao-senha'),
        larguraLogica: 1024,
        altura: 460,
        hotspots: [
          { numero: 4, xPct: 58, yPct: 7, dica: 'Menu inativo e sem identificação da conta' },
          { numero: 3, xPct: 45, yPct: 46, dica: 'Pede a senha ATUAL no formulário' },
        ],
      },
    ],
    cards: [
      {
        numero: 1,
        icone: '🔤',
        titulo: 'Inversão do Nome da Marca',
        pontos: [
          "O e-mail e a página utilizam 'bho MAX'.",
          'Leitura rápida e desatenção costumam esconder pequenas alterações de letras nas marcas.',
        ],
      },
      {
        numero: 2,
        icone: '⏰',
        titulo: 'Pressão de Tempo e Padrão Estrangeiro',
        pontos: [
          "O horário aparece em fuso 'EST' (ex.: 2:19pm EST) — um padrão estrangeiro que destoa de comunicações locais legítimas.",
          "Combinado a um prazo curto de expiração ('ainda hoje'), cria um gatilho emocional de urgência para induzir a ação impulsiva antes da verificação.",
        ],
      },
      {
        numero: 3,
        icone: '🔑',
        titulo: 'Captura da Credencial Ativa (Senha Atual)',
        pontos: [
          "A página solicita a 'Senha atual' no formulário de redefinição.",
          'Esse é o objetivo do atacante: obter sua senha válida para invadir sua conta.',
        ],
      },
      {
        numero: 4,
        icone: '🚫',
        titulo: 'Elementos Estáticos e Ausência do Seu E-mail',
        pontos: [
          'A tela de alteração não especifica para qual conta a senha está sendo alterada.',
          'Os links de navegação do menu superior não funcionam.',
        ],
      },
      {
        numero: 5,
        icone: '📧',
        titulo: 'Domínio do Remetente Falsificado',
        pontos: [
          "O e-mail vem de 'no-reply@alerts.bhomax.com' — um subdomínio de aparência oficial, mas não é o domínio real do serviço.",
          'Confira sempre o domínio completo do remetente antes de agir.',
        ],
      },
    ],
    conclusao: {
      label: 'Concluir Treinamento',
      redirecionarPara: '/',
    },
  },

  mercadoliv: {
    id: 'mercadoliv',
    marca: 'Mercado Liv',
    header: {
      badge: 'Atenção: Esta foi uma simulação de treinamento de segurança do PhishGuard.',
      titulo: 'Você interagiu com um e-mail de phishing simulado.',
      mensagem:
        'Não se preocupe: nenhum dado real foi comprometido — apenas o clique foi ' +
        'registrado. Use os pontos destacados abaixo para aprender a identificar esse ' +
        'tipo de ataque no futuro.',
    },
    mockups: [
      {
        id: 'email',
        titulo: 'O e-mail que você recebeu',
        chrome: {
          tipo: 'email',
          remetenteNome: 'Mercado Liv',
          remetenteEmail: 'no-reply@mercadoliv.com',
          assunto: 'Detectamos um novo acesso à sua conta',
        },
        html: resolverEmailHtml('mercado-liv-novo-acesso'),
        larguraLogica: 600,
        altura: 460,
        hotspots: [
          { numero: 1, xPct: 26, yPct: 3, dica: 'Domínio do remetente falsificado' },
          { numero: 2, xPct: 55, yPct: 44, dica: 'Alerta de acesso com data/hora fabricada' },
          { numero: 3, xPct: 30, yPct: 68, dica: 'Botão de ação com destino suspeito' },
        ],
      },
      {
        id: 'landing',
        titulo: 'A página falsa de login',
        chrome: {
          tipo: 'navegador',
          url: 'mercadoliv-seguranca-conta.help/entrar',
          seguro: false,
        },
        html: resolverLandingHtml('mercado-liv-login'),
        larguraLogica: 1024,
        altura: 460,
        hotspots: [
          { numero: 4, xPct: 74, yPct: 48, dica: 'Pede e-mail e senha atual direto' },
          { numero: 5, xPct: 26, yPct: 56, dica: 'Selos e links de ajuda decorativos' },
        ],
      },
    ],
    cards: [
      {
        numero: 1,
        icone: '📧',
        titulo: 'Domínio do Remetente Falsificado',
        pontos: [
          "O e-mail vem de 'no-reply@mercadoliv.com' — imita a marca, mas não é o domínio oficial do serviço.",
          'Confira sempre o domínio completo do remetente antes de agir.',
        ],
      },
      {
        numero: 2,
        icone: '⏰',
        titulo: 'Alerta de Acesso com Urgência',
        pontos: [
          "A mensagem afirma detectar um 'novo acesso' com data e hora específicas para gerar medo e pressa.",
          'Detalhes fabricados (data/hora/dispositivo) dão falsa credibilidade e induzem a ação impulsiva.',
        ],
      },
      {
        numero: 3,
        icone: '🔗',
        titulo: 'Botão de Ação Suspeito',
        pontos: [
          "O botão 'Redefinir Senha de Acesso' leva a um endereço fora do serviço oficial.",
          'Passe o mouse sobre o botão e confira a URL real antes de clicar.',
        ],
      },
      {
        numero: 4,
        icone: '🔑',
        titulo: 'Captura de E-mail e Senha Atual',
        pontos: [
          "A página pede seu e-mail e a 'senha atual' logo na primeira tela, sem verificação em duas etapas (2FA).",
          'É exatamente a credencial válida que o atacante precisa para invadir sua conta.',
        ],
      },
      {
        numero: 5,
        icone: '🛡️',
        titulo: 'Falso Senso de Segurança',
        pontos: [
          "Selos como 'Protegido por reCAPTCHA' e botões de ajuda ('Tenho um problema de segurança', 'Preciso de ajuda') são decorativos e não funcionam.",
          'Aparência de legitimidade não substitui a verificação do endereço real da página.',
        ],
      },
    ],
    conclusao: {
      label: 'Concluir Treinamento',
      redirecionarPara: '/',
    },
  },

  microsft365: {
    id: 'microsft365',
    marca: 'Microsft 365',
    header: {
      badge: 'Atenção: Esta foi uma simulação de treinamento de segurança do PhishGuard.',
      titulo: 'Você interagiu com um e-mail de phishing simulado.',
      mensagem:
        'Nenhuma credencial corporativa foi alterada. Analise os pontos abaixo para ' +
        'identificar como este ataque explora rotinas de TI.',
    },
    mockups: [
      {
        id: 'email',
        titulo: 'O e-mail que você recebeu',
        chrome: {
          tipo: 'email',
          remetenteNome: 'Microsft 365',
          remetenteEmail: 'no-reply@microsft365.com',
          assunto: 'Ação Necessária: Expiração de Senha',
        },
        html: resolverEmailHtml('microcorp-expiracao-senha'),
        larguraLogica: 600,
        altura: 460,
        hotspots: [
          { numero: 1, xPct: 20, yPct: 9, dica: 'Nome da marca com erro de digitação' },
          { numero: 2, xPct: 52, yPct: 34, dica: 'Urgência artificial: expira em 24 horas' },
          { numero: 3, xPct: 30, yPct: 60, dica: 'Botão leva a uma página externa' },
          { numero: 4, xPct: 25, yPct: 74, dica: 'Assinatura de TI impessoal' },
        ],
      },
      {
        id: 'landing',
        titulo: 'A página falsa de login corporativo',
        chrome: {
          tipo: 'navegador',
          url: 'microsft365-portal-seguranca.help/entrar',
          seguro: false,
        },
        html: resolverLandingHtml('microcorp-login'),
        larguraLogica: 1024,
        altura: 460,
        hotspots: [
          { numero: 5, xPct: 50, yPct: 55, dica: 'Pede e-mail e senha atual, sem 2FA' },
        ],
      },
    ],
    cards: [
      {
        numero: 1,
        icone: '🔤',
        titulo: 'Erro Sutil no Nome da Empresa',
        pontos: [
          "Observe a omissão da letra 'o' em 'Microsft 365'.",
          'Atacantes usam pequenos erros de digitação (typosquatting) para enganar a leitura rápida do usuário.',
        ],
      },
      {
        numero: 2,
        icone: '⏰',
        titulo: 'Falso Alerta de Expiração de Senha',
        pontos: [
          "Avisos de expiração em '24 horas' criam senso de urgência para que o colaborador clique no botão antes de checar os canais oficiais de TI da empresa.",
          'Na dúvida, procure o suporte de TI pelos canais internos oficiais — nunca pelo botão do e-mail.',
        ],
      },
      {
        numero: 3,
        icone: '🔗',
        titulo: 'Link Externo para Captura de Credenciais',
        pontos: [
          "O botão 'Manter minha senha atual' direciona para uma página externa de login que simula o portal da empresa para roubar a senha da sua conta corporativa.",
          'Passe o mouse sobre o botão e confira o destino real antes de clicar.',
        ],
      },
      {
        numero: 4,
        icone: '📧',
        titulo: 'Remetente Automático e Sem Assinatura Oficial',
        pontos: [
          "O remetente 'no-reply@microsft365.com' imita a marca, mas não é o domínio corporativo real da sua empresa.",
          "O rodapé traz apenas 'Equipe de TI' e 'e-mail automático', sem nome, ramal ou número de chamado — comunicações legítimas de TI costumam se identificar.",
        ],
      },
      {
        numero: 5,
        icone: '🔑',
        titulo: 'Portal Falso de Captura de Senha',
        pontos: [
          'A página pede seu e-mail e a senha atual da conta institucional, sem verificação em duas etapas (2FA).',
          'É exatamente a credencial válida que o atacante precisa para acessar a rede corporativa.',
        ],
      },
    ],
    conclusao: {
      label: 'Concluir Treinamento',
      redirecionarPara: '/',
    },
  },
};
