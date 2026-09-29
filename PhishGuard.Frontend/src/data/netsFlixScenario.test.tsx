import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { templatesPredefinidos } from './predefinedTemplates';
import { landingTemplates } from './landingTemplates';
import { feedbackTrainings } from './feedbackTrainings';
import FeedbackTraining from '../components/FeedbackTraining';

// ============================================================================
// Suíte do cenário Netflix restaurado a partir de 631acbe^:
//   • E-mail (netflix-atualizacao-cobranca): logotipo e nome históricos.
//   • Página falsa (netflix-login): captura e-mail + senha.
//   • Tela educacional (feedbackTrainings.netsflix).
//
// O ID de feedback netsflix permanece compatível com campanhas existentes.
// ============================================================================

const { navigateMock } = vi.hoisted(() => ({ navigateMock: vi.fn() }));
vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-router-dom')>();
  return { ...actual, useNavigate: () => navigateMock };
});

const IDS = { email: 'netflix-atualizacao-cobranca', landing: 'netflix-login', feedback: 'netsflix' } as const;
const CAMP = 'camp-1';
const TGT = 'tgt-1';
const TRACKING_TOKEN = 'token-assinado';
const CLICK = 'https://phishguard.example/api/tracking/click/camp-1/tgt-1';

describe('NetsFlix — Template de E-mail', () => {
  function emailComProps() {
    const isca = templatesPredefinidos.find((t) => t.id === IDS.email);
    if (!isca) throw new Error('isca netflix não encontrada');
    return isca.corpoHtml.replaceAll('{{LINK_PHISHING}}', CLICK);
  }

  it('renderiza com {{LINK_PHISHING}} substituído e sem placeholders remanescentes', () => {
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: emailComProps() }} />);
    expect(container.innerHTML).toContain(CLICK);
    expect(container.innerHTML).not.toMatch(/\{\{.*?\}\}/);
  });

  it('restaura o logotipo histórico da Netflix', () => {
    const isca = templatesPredefinidos.find((t) => t.id === IDS.email)!;
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: emailComProps() }} />);
    expect(container.querySelector('img[alt="Netflix"]')).toHaveAttribute('src', expect.stringContaining('assets.nflxext.com'));
    expect(isca.corpoHtml).not.toContain('NetsFlix');
  });

  it('restaura o nome do remetente e do catálogo para Netflix', () => {
    const isca = templatesPredefinidos.find((t) => t.id === IDS.email)!;
    // O nome do remetente é copiado para o
    // header From no disparo (CampaignDispatchService → new MailboxAddress(RemetenteNome, ...)).
    expect(isca.remetenteNome).toBe('Netflix');
    expect(isca.remetenteEmail).toBe('info@account.netflix.com');
    expect(isca.nome).toMatch(/Netflix/);
  });

  it('usa a marca histórica no corpo do e-mail', () => {
    const html = emailComProps();
    // Confere a grafia histórica da marca.
    expect(html).toMatch(/\bNetflix\b/);
    expect(html).not.toMatch(/paród|fictíci|simulação de conscientiz/i);
  });
});

describe('NetsFlix — Página Simulada (Phishing)', () => {
  function landingHtml() {
    const l = landingTemplates.find((x) => x.id === IDS.landing);
    if (!l) throw new Error('landing netflix não encontrada');
    return l.html.replaceAll('{{CAMPAIGN_ID}}', CAMP).replaceAll('{{TARGET_ID}}', TGT).replaceAll('{{TRACKING_TOKEN}}', TRACKING_TOKEN);
  }

  it('renderiza o formulário de captura (e-mail + senha)', () => {
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: landingHtml() }} />);
    expect(container.querySelector('form')).toBeInTheDocument();
    expect(container.querySelector('#nfx-email')).toBeInTheDocument();
    expect(container.querySelector('#nfx-password')).toBeInTheDocument();
  });

  it('ao submeter: envia só metadados (LGPD) para /api/tracking/submit e redireciona ao treinamento', async () => {
    const fetchMock = vi.fn(() => Promise.resolve({ ok: true, json: async () => ({}) }));
    vi.stubGlobal('fetch', fetchMock);
    const loc: { href: string; search: string } = { href: '', search: '' };
    const orig = Object.getOwnPropertyDescriptor(window, 'location');
    Object.defineProperty(window, 'location', { configurable: true, value: loc });
    try {
      const { container } = render(<div dangerouslySetInnerHTML={{ __html: landingHtml() }} />);
      (container.querySelector('#nfx-email') as HTMLInputElement).value = 'vitima@exemplo.com';
      (container.querySelector('#nfx-password') as HTMLInputElement).value = 'segredo12';

      const form = container.querySelector('form') as HTMLFormElement;
      const onsubmit = form.getAttribute('onsubmit');
      expect(onsubmit).toBeTruthy();
      new Function('event', onsubmit as string).call(form, { preventDefault: vi.fn() });

      await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1));
      const [url, opts] = fetchMock.mock.calls[0] as unknown as [string, RequestInit];
      expect(url).toBe(`/api/tracking/submit/${CAMP}/${TGT}?k=${TRACKING_TOKEN}`);
      expect(opts.method).toBe('POST');
      const body = JSON.parse(opts.body as string);
      expect(body).toMatchObject({ camposPreenchidos: true, emailInformado: true, tamanhoSenha: 'segredo12'.length });
      // LGPD: sem e-mail/senha reais no payload.
      expect(opts.body as string).not.toContain('vitima@exemplo.com');
      expect(opts.body as string).not.toContain('segredo12');

      await waitFor(() =>
        expect(loc.href).toBe(`/educational-feedback?template=netsflix&c=${CAMP}&t=${TGT}&k=${TRACKING_TOKEN}`),
      );
    } finally {
      if (orig) Object.defineProperty(window, 'location', orig);
      vi.unstubAllGlobals();
    }
  });
});

describe('NetsFlix — Tela Educacional (Just-in-Time)', () => {
  beforeEach(() => {
    navigateMock.mockClear();
    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve({ ok: true, json: async () => ({}) })));
  });
  afterEach(() => vi.unstubAllGlobals());

  function renderEdu() {
    render(
      <MemoryRouter initialEntries={[`/educational-feedback?c=${CAMP}&t=${TGT}&k=${TRACKING_TOKEN}`]}>
        <FeedbackTraining config={feedbackTrainings[IDS.feedback]} />
      </MemoryRouter>,
    );
  }

  it('exibe a conscientização e os vetores de ataque do cenário', () => {
    renderEdu();
    expect(screen.getByText(/simulação de treinamento de segurança do PhishGuard/i)).toBeInTheDocument();
    expect(screen.getByText(/Você interagiu com um e-mail de phishing simulado/i)).toBeInTheDocument();
    expect(screen.getByText(/Falsa Exigência Legal/i)).toBeInTheDocument();
    expect(screen.getByText(/Saudação Genérica e Identidade Visual Copiada/i)).toBeInTheDocument();
    expect(screen.getByText(/Botão de Ação Suspeito/i)).toBeInTheDocument();
    expect(screen.getByText(/Solicitação Direta de Senha na Tela Inicial/i)).toBeInTheDocument();
  });

  it('o botão de conclusão registra a participação e encerra o fluxo', async () => {
    const fetchMock = vi.fn(() => Promise.resolve({ ok: true, json: async () => ({}) }));
    vi.stubGlobal('fetch', fetchMock);
    renderEdu();
    fireEvent.click(screen.getByRole('button', { name: /Concluir Treinamento/i }));
    await waitFor(() =>
      expect(fetchMock).toHaveBeenCalledWith(
        `/api/tracking/complete/${CAMP}/${TGT}?k=${TRACKING_TOKEN}`,
        expect.objectContaining({ method: 'POST' }),
      ),
    );
    await waitFor(() => expect(navigateMock).toHaveBeenCalledWith('/'));
  });
});
