import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { templatesPredefinidos } from './predefinedTemplates';
import { landingTemplates } from './landingTemplates';
import { feedbackTrainings } from './feedbackTrainings';
import FeedbackTraining from '../components/FeedbackTraining';

// ============================================================================
// Suíte do CENÁRIO "NetsFlix" — mesmo BLUEPRINT, adaptado à isca:
//   • E-mail (netflix-atualizacao-cobranca): identidade é um WORDMARK textual/CSS
//     (compliance de IP — SEM logo raster/cid); placeholder {{LINK_PHISHING}}.
//   • Página falsa (netflix-login): captura e-mail + senha.
//   • Tela educacional (feedbackTrainings.netsflix).
//
// GUARDRAIL DE MARCA: a identidade exibida ao alvo (nome do remetente, corpo do e-mail)
// usa ESTRITAMENTE "NetsFlix" e nunca a marca real nem qualquer aviso de simulação —
// ver [[dashboard-v2]]/CLAUDE.md (compliance de IP). Os testes abaixo travam isso.
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

  it('usa wordmark textual (sem logo raster/cid) e conforma a marca "NetsFlix"', () => {
    const isca = templatesPredefinidos.find((t) => t.id === IDS.email)!;
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: emailComProps() }} />);
    // Marca exibida presente no corpo…
    expect(container.textContent).toContain('NetsFlix');
    // …e NENHUM logo raster/cid embutido (identidade puramente textual/CSS).
    expect(isca.corpoHtml).not.toContain('data:image/png');
    expect(isca.corpoHtml).not.toContain('cid:');
  });

  it('o NOME DE EXIBIÇÃO do remetente é estritamente "NetsFlix" (nunca a marca real)', () => {
    const isca = templatesPredefinidos.find((t) => t.id === IDS.email)!;
    // Bug reportado: o From chegava como "Netflix". O nome do remetente é copiado para o
    // header From no disparo (CampaignDispatchService → new MailboxAddress(RemetenteNome, ...)).
    expect(isca.remetenteNome).toBe('NetsFlix');
    // Marca REAL não pode aparecer em NENHUM campo visível ao alvo.
    expect(isca.remetenteNome).not.toMatch(/netflix/i);
    expect(isca.nome).not.toMatch(/netflix/i);
  });

  it('não vaza a marca real nem aviso de simulação/paródia no corpo do e-mail', () => {
    const html = emailComProps();
    // \b evita casar "NetsFlix"; procura a grafia REAL "Netflix" isolada.
    expect(html).not.toMatch(/\bNetflix\b/);
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
    expect(screen.getByText(/Saudação Genérica e Alteração da Marca/i)).toBeInTheDocument();
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
