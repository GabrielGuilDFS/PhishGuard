import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { templatesPredefinidos } from './predefinedTemplates';
import { landingTemplates } from './landingTemplates';
import { feedbackTrainings } from './feedbackTrainings';
import { formatarDataAcessoBRT } from '../utils/emailExpiration';
import FeedbackTraining from '../components/FeedbackTraining';

// ============================================================================
// Suíte do CENÁRIO "Mercado Liv" — as três telas finalizadas:
//   1) Template de E-mail  (data: predefinedTemplates.corpoHtml, espelha o recurso
//      embutido do backend; o disparo real é coberto por xUnit — OfficialBaitCatalog).
//   2) Página Simulada     (data: landingTemplates, servida por LandingPage.tsx).
//   3) Tela Educacional    (componente genérico FeedbackTraining + config em
//      feedbackTrainings, idêntico a NetsFlix/bho MAX/Microsft 365).
//
// Padrão do projeto: Vitest + React Testing Library, co-located (mirroring), fetch e
// navegação mockados para isolar as unidades. Os data-testid conferem 1:1 com o HTML
// implementado (ml-header/ml-logo/ml-body/ml-device/ml-cta).
// ============================================================================

// Mock APENAS de useNavigate (mantém MemoryRouter/useSearchParams reais), para auditar
// o redirecionamento de conclusão do treinamento sem navegar de verdade.
const { navigateMock } = vi.hoisted(() => ({ navigateMock: vi.fn() }));
vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-router-dom')>();
  return { ...actual, useNavigate: () => navigateMock };
});

const IDS = {
  email: 'mercadoliv-novo-acesso',
  landing: 'mercadoliv-login',
  feedback: 'mercadoliv',
} as const;
const CAMP = 'camp-1';
const TGT = 'tgt-1';
const TRACKING_TOKEN = 'token-assinado';

// ----------------------------------------------------------------------------
// 1) TEMPLATE DE E-MAIL
// ----------------------------------------------------------------------------
describe('Mercado Liv — Template de E-mail', () => {
  // Aplica as variáveis dinâmicas como o previewer/disparo fazem: {{NOME}} (nome do
  // alvo), {{LINK_PHISHING}} (URL de clique/track) e {{DATA_ACESSO}} (data no fuso BRT).
  function emailComProps(nome: string, click: string, data: string) {
    const isca = templatesPredefinidos.find((t) => t.id === IDS.email);
    if (!isca) throw new Error('isca mercadoliv-novo-acesso não encontrada');
    return isca.corpoHtml
      .replaceAll('{{NOME}}', nome)
      .replaceAll('{{LINK_PHISHING}}', click)
      .replaceAll('{{DATA_ACESSO}}', data);
  }

  it('renderiza o HTML com as props do usuário e sem placeholders remanescentes', () => {
    const nome = 'Fulano de Tal';
    const click = 'https://phishguard.example/api/tracking/click/CID-1/TID-1';
    const data = formatarDataAcessoBRT(new Date('2026-01-15T09:30:00'));
    const { container } = render(
      <div dangerouslySetInnerHTML={{ __html: emailComProps(nome, click, data) }} />,
    );

    // Estrutura via data-testid (conferem com o HTML implementado).
    expect(screen.getByTestId('ml-header')).toBeInTheDocument();
    expect(screen.getByTestId('ml-logo')).toBeInTheDocument();
    expect(screen.getByTestId('ml-cta')).toBeInTheDocument();

    // Props do usuário aplicadas.
    expect(screen.getByTestId('ml-body').textContent).toContain(nome);
    expect(screen.getByTestId('ml-cta')).toHaveAttribute('href', click);
    expect(screen.getByTestId('ml-cta').textContent).toMatch(/Redefinir Senha/i);

    // Nenhum placeholder cru sobrando.
    expect(container.innerHTML).not.toMatch(/\{\{.*?\}\}/);
  });

  it('marca em caixa baixa "mercado/liv" e logo injetada como PNG (fallback data-URI no preview)', () => {
    render(<div dangerouslySetInnerHTML={{ __html: emailComProps('X', '#', formatarDataAcessoBRT()) }} />);

    // Wordmark estritamente minúsculo (o cid usa "mercado" + "liv", sem "Mercado Liv").
    const header = screen.getByTestId('ml-header');
    expect(header.textContent?.replace(/\s+/g, '')).toBe('mercadoliv');
    expect(header.textContent).not.toMatch(/Mercado Liv/);

    // No previewer a logo é o PNG em data-URI (fallback); no disparo real vira cid:logo-mercadoliv.
    const logo = screen.getByTestId('ml-logo') as HTMLImageElement;
    expect(logo.getAttribute('src')).toMatch(/^data:image\/png;base64,/);
    expect(logo).toHaveAttribute('alt', 'Mercado Liv');
  });

  it('exibe data/hora no padrão do fuso de Brasília (America/Sao_Paulo → "(BRT)")', () => {
    const data = formatarDataAcessoBRT(new Date('2026-03-10T14:05:00'));
    render(<div dangerouslySetInnerHTML={{ __html: emailComProps('X', '#', data) }} />);

    const device = screen.getByTestId('ml-device').textContent ?? '';
    expect(device).toMatch(/\d{2}\/\d{2}\/\d{4} às \d{2}:\d{2} \(BRT\)/);
    expect(device).toContain(data);
  });
});

// ----------------------------------------------------------------------------
// 2) PÁGINA SIMULADA (PHISHING / FAKE PAGE)
// ----------------------------------------------------------------------------
describe('Mercado Liv — Página Simulada (Phishing)', () => {
  // LandingPage.tsx substitui {{CAMPAIGN_ID}}/{{TARGET_ID}} antes de injetar o HTML.
  function landingHtml() {
    const l = landingTemplates.find((x) => x.id === IDS.landing);
    if (!l) throw new Error('landing mercadoliv-login não encontrada');
    return l.html.replaceAll('{{CAMPAIGN_ID}}', CAMP).replaceAll('{{TARGET_ID}}', TGT).replaceAll('{{TRACKING_TOKEN}}', TRACKING_TOKEN);
  }

  it('renderiza o formulário de captura de credenciais', () => {
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: landingHtml() }} />);

    expect(container.querySelector('form')).toBeInTheDocument();
    expect(container.querySelector('#ml-email')).toBeInTheDocument();
    expect(container.querySelector('#ml-password')).toBeInTheDocument();
    expect(screen.getByText('Continuar')).toBeInTheDocument();
    // Identidade da marca em caixa baixa no cabeçalho.
    expect(container.textContent?.replace(/\s+/g, '')).toContain('mercadoliv');
  });

  it('ao submeter: chama a API de comprometimento com os args corretos e redireciona ao treinamento', async () => {
    const fetchMock = vi.fn(() => Promise.resolve({ ok: true, json: async () => ({}) }));
    vi.stubGlobal('fetch', fetchMock);

    // Isola a navegação: substitui window.location por um stub gravável.
    const loc: { href: string; search: string } = { href: '', search: '' };
    const originalLocation = Object.getOwnPropertyDescriptor(window, 'location');
    Object.defineProperty(window, 'location', { configurable: true, value: loc });

    try {
      const { container } = render(<div dangerouslySetInnerHTML={{ __html: landingHtml() }} />);
      (container.querySelector('#ml-email') as HTMLInputElement).value = 'vitima@exemplo.com';
      (container.querySelector('#ml-password') as HTMLInputElement).value = 'senha123';

      // O handler de submit é um atributo inline `onsubmit` (funciona sob
      // dangerouslySetInnerHTML no app). Executamos seu corpo de forma determinística.
      const form = container.querySelector('form') as HTMLFormElement;
      const onsubmit = form.getAttribute('onsubmit');
      expect(onsubmit).toBeTruthy();
      const handler = new Function('event', onsubmit as string);
      handler.call(form, { preventDefault: vi.fn() });

      // Disparo da telemetria de comprometimento com os argumentos corretos.
      await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1));
      const [url, opts] = fetchMock.mock.calls[0] as unknown as [string, RequestInit];
      expect(url).toBe(`/api/tracking/submit/${CAMP}/${TGT}?k=${TRACKING_TOKEN}`);
      expect(opts.method).toBe('POST');
      const body = JSON.parse(opts.body as string);
      expect(body).toMatchObject({ camposPreenchidos: true, tamanhoSenha: 'senha123'.length });

      // LGPD: só metadados — nunca a credencial real no payload.
      expect(opts.body as string).not.toContain('vitima@exemplo.com');
      expect(opts.body as string).not.toContain('senha123');

      // Redireciona para a Tela Educacional do cenário, preservando c/t.
      await waitFor(() =>
        expect(loc.href).toBe(`/educational-feedback?template=mercadoliv&c=${CAMP}&t=${TGT}&k=${TRACKING_TOKEN}`),
      );
    } finally {
      if (originalLocation) Object.defineProperty(window, 'location', originalLocation);
      vi.unstubAllGlobals();
    }
  });
});

// ----------------------------------------------------------------------------
// 3) TELA EDUCACIONAL (JUST-IN-TIME TRAINING)
// ----------------------------------------------------------------------------
describe('Mercado Liv — Tela Educacional (Just-in-Time)', () => {
  beforeEach(() => {
    navigateMock.mockClear();
    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve({ ok: true, json: async () => ({}) })));
  });
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  function renderEdu() {
    render(
      <MemoryRouter initialEntries={[`/educational-feedback?c=${CAMP}&t=${TGT}&k=${TRACKING_TOKEN}`]}>
        <FeedbackTraining config={feedbackTrainings[IDS.feedback]} />
      </MemoryRouter>,
    );
  }

  it('exibe a mensagem de conscientização e a lista de vetores de ataque', () => {
    renderEdu();

    // Cabeçalho de alerta/conscientização.
    expect(
      screen.getByText(/simulação de treinamento de segurança do PhishGuard/i),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/Você interagiu com um e-mail de phishing simulado/i),
    ).toBeInTheDocument();

    // Vetores de ataque (cards) exigidos para o cenário de e-commerce.
    expect(screen.getByText(/Domínio do Remetente/i)).toBeInTheDocument();
    expect(screen.getByText(/Urgência e Alerta Falso/i)).toBeInTheDocument();
    expect(screen.getByText(/Links e Botões/i)).toBeInTheDocument();
    expect(screen.getByText(/Página Falsa sem 2FA/i)).toBeInTheDocument();
    expect(screen.getByText(/Boas Práticas/i)).toBeInTheDocument();
  });

  it('o botão de conclusão registra a participação (auditoria) e encerra o fluxo', async () => {
    const fetchMock = vi.fn(() => Promise.resolve({ ok: true, json: async () => ({}) }));
    vi.stubGlobal('fetch', fetchMock);

    renderEdu();
    const botao = screen.getByRole('button', { name: /Concluir Treinamento/i });
    expect(botao).toBeInTheDocument();
    fireEvent.click(botao);

    await waitFor(() =>
      expect(fetchMock).toHaveBeenCalledWith(
        `/api/tracking/complete/${CAMP}/${TGT}?k=${TRACKING_TOKEN}`,
        expect.objectContaining({ method: 'POST' }),
      ),
    );
    await waitFor(() => expect(navigateMock).toHaveBeenCalledWith('/'));
  });
});
