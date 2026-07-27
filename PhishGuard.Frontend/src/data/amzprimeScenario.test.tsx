import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { templatesPredefinidos } from './predefinedTemplates';
import { landingTemplates } from './landingTemplates';
import { feedbackTrainings } from './feedbackTrainings';
import FeedbackTraining from '../components/FeedbackTraining';

// ============================================================================
// Suíte do CENÁRIO "amzprime" (paródia FICTÍCIA da Amazon — o "Amasson" do pedido).
// Mesmo BLUEPRINT, adaptado à isca real:
//   • E-mail (amazon-notificacao-seguranca): identidade é WORDMARK textual
//     ("amz"+"prime", CSS — sem logo raster/cid); placeholder {{LINK_PHISHING}}.
//   • Página falsa (amazon-login): captura nova senha + confirmação.
//   • Tela educacional (feedbackTrainings.amzprime).
// ============================================================================

const { navigateMock } = vi.hoisted(() => ({ navigateMock: vi.fn() }));
vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-router-dom')>();
  return { ...actual, useNavigate: () => navigateMock };
});

const IDS = { email: 'amazon-notificacao-seguranca', landing: 'amazon-login', feedback: 'amzprime' } as const;
const CAMP = 'camp-1';
const TGT = 'tgt-1';
const CLICK = 'https://phishguard.example/api/tracking/click/camp-1/tgt-1';

describe('amzprime — Template de E-mail', () => {
  function emailComProps() {
    const isca = templatesPredefinidos.find((t) => t.id === IDS.email);
    if (!isca) throw new Error('isca amzprime não encontrada');
    return isca.corpoHtml.replaceAll('{{LINK_PHISHING}}', CLICK);
  }

  it('renderiza com {{LINK_PHISHING}} substituído e sem placeholders remanescentes', () => {
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: emailComProps() }} />);
    expect(container.innerHTML).toContain(CLICK);
    expect(container.innerHTML).not.toMatch(/\{\{.*?\}\}/);
  });

  it('usa wordmark textual parodiado (sem logo raster/cid) e conforma a marca "amzprime"', () => {
    const isca = templatesPredefinidos.find((t) => t.id === IDS.email)!;
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: emailComProps() }} />);
    expect(container.textContent).toContain('amzprime');
    expect(isca.corpoHtml).not.toContain('data:image/png');
    expect(isca.corpoHtml).not.toContain('cid:');
  });
});

describe('amzprime — Página Simulada (Phishing)', () => {
  function landingHtml() {
    const l = landingTemplates.find((x) => x.id === IDS.landing);
    if (!l) throw new Error('landing amazon-login não encontrada');
    return l.html.replaceAll('{{CAMPAIGN_ID}}', CAMP).replaceAll('{{TARGET_ID}}', TGT);
  }

  it('renderiza o formulário de captura (nova senha + confirmação)', () => {
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: landingHtml() }} />);
    expect(container.querySelector('form')).toBeInTheDocument();
    expect(container.querySelector('#amz-new')).toBeInTheDocument();
    expect(container.querySelector('#amz-confirm')).toBeInTheDocument();
  });

  it('ao submeter: envia só metadados (LGPD) para /api/tracking/submit e redireciona ao treinamento', async () => {
    const fetchMock = vi.fn(() => Promise.resolve({ ok: true, json: async () => ({}) }));
    vi.stubGlobal('fetch', fetchMock);
    const loc: { href: string; search: string } = { href: '', search: '' };
    const orig = Object.getOwnPropertyDescriptor(window, 'location');
    Object.defineProperty(window, 'location', { configurable: true, value: loc });
    try {
      const { container } = render(<div dangerouslySetInnerHTML={{ __html: landingHtml() }} />);
      (container.querySelector('#amz-new') as HTMLInputElement).value = 'Senha123';
      (container.querySelector('#amz-confirm') as HTMLInputElement).value = 'Senha123';

      const form = container.querySelector('form') as HTMLFormElement;
      const onsubmit = form.getAttribute('onsubmit');
      expect(onsubmit).toBeTruthy();
      new Function('event', onsubmit as string).call(form, { preventDefault: vi.fn() });

      await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1));
      const [url, opts] = fetchMock.mock.calls[0] as unknown as [string, RequestInit];
      expect(url).toBe(`/api/tracking/submit/${CAMP}/${TGT}`);
      expect(opts.method).toBe('POST');
      const body = JSON.parse(opts.body as string);
      expect(body).toMatchObject({ camposPreenchidos: true, senhasCoincidem: true, tamanhoSenha: 'Senha123'.length });
      // LGPD: nenhuma senha em texto no payload.
      expect(opts.body as string).not.toContain('Senha123');

      await waitFor(() =>
        expect(loc.href).toBe(`/educational-feedback?template=amzprime&c=${CAMP}&t=${TGT}`),
      );
    } finally {
      if (orig) Object.defineProperty(window, 'location', orig);
      vi.unstubAllGlobals();
    }
  });
});

describe('amzprime — Tela Educacional (Just-in-Time)', () => {
  beforeEach(() => {
    navigateMock.mockClear();
    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve({ ok: true, json: async () => ({}) })));
  });
  afterEach(() => vi.unstubAllGlobals());

  function renderEdu() {
    render(
      <MemoryRouter initialEntries={['/educational-feedback?c=camp-1&t=tgt-1']}>
        <FeedbackTraining config={feedbackTrainings[IDS.feedback]} />
      </MemoryRouter>,
    );
  }

  it('exibe a conscientização e os vetores de ataque do cenário', () => {
    renderEdu();
    expect(screen.getByText(/simulação de treinamento de segurança do PhishGuard/i)).toBeInTheDocument();
    expect(screen.getByText(/Você interagiu com um e-mail de phishing simulado/i)).toBeInTheDocument();
    expect(screen.getByText(/Remetente e saudação suspeitos/i)).toBeInTheDocument();
    expect(screen.getByText(/Gatilho de urgência/i)).toBeInTheDocument();
    expect(screen.getByText(/URL da página falsa/i)).toBeInTheDocument();
    expect(screen.getByText(/Sem 2FA e links inativos/i)).toBeInTheDocument();
    expect(screen.getByText(/Pedido incoerente de senha/i)).toBeInTheDocument();
    expect(screen.getByText(/CNPJ e logotipo/i)).toBeInTheDocument();
  });

  it('o botão de conclusão registra a participação e encerra o fluxo', async () => {
    const fetchMock = vi.fn(() => Promise.resolve({ ok: true, json: async () => ({}) }));
    vi.stubGlobal('fetch', fetchMock);
    renderEdu();
    fireEvent.click(screen.getByRole('button', { name: /Concluir Treinamento/i }));
    await waitFor(() =>
      expect(fetchMock).toHaveBeenCalledWith(
        '/api/tracking/complete/camp-1/tgt-1',
        expect.objectContaining({ method: 'POST' }),
      ),
    );
    await waitFor(() => expect(navigateMock).toHaveBeenCalledWith('/'));
  });
});
