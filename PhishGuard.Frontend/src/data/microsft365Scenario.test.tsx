import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { templatesPredefinidos } from './predefinedTemplates';
import { landingTemplates } from './landingTemplates';
import { feedbackTrainings } from './feedbackTrainings';
import { formatarDataAcessoBRT } from '../utils/emailExpiration';
import FeedbackTraining from '../components/FeedbackTraining';

// ============================================================================
// Suíte do CENÁRIO "Microsft 365" (paródia corporativa; typosquatting proposital).
// Mesmo BLUEPRINT, adaptado à isca real:
//   • E-mail (microcorp-expiracao-senha): logotipo-paródia = grid 2x2 de quadrados
//     coloridos (CSS/<table>, sem logo raster/cid); placeholders {{NOME}},
//     {{LINK_PHISHING}} e {{DATA_ACESSO}} (fuso de Brasília).
//   • Página falsa (microcorp-login): captura e-mail + senha corporativos.
//   • Tela educacional (feedbackTrainings.microsft365).
// ============================================================================

const { navigateMock } = vi.hoisted(() => ({ navigateMock: vi.fn() }));
vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-router-dom')>();
  return { ...actual, useNavigate: () => navigateMock };
});

const IDS = { email: 'microcorp-expiracao-senha', landing: 'microcorp-login', feedback: 'microsft365' } as const;
const CAMP = 'camp-1';
const TGT = 'tgt-1';
const CLICK = 'https://phishguard.example/api/tracking/click/camp-1/tgt-1';

describe('Microsft 365 — Template de E-mail', () => {
  function emailComProps(nome: string, data: string) {
    const isca = templatesPredefinidos.find((t) => t.id === IDS.email);
    if (!isca) throw new Error('isca microcorp não encontrada');
    return isca.corpoHtml
      .replaceAll('{{NOME}}', nome)
      .replaceAll('{{LINK_PHISHING}}', CLICK)
      .replaceAll('{{DATA_ACESSO}}', data);
  }

  it('substitui {{NOME}}/{{LINK_PHISHING}}/{{DATA_ACESSO}} e não deixa placeholders remanescentes', () => {
    const nome = 'Fulano de Tal';
    const data = formatarDataAcessoBRT(new Date('2026-01-15T09:30:00'));
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: emailComProps(nome, data) }} />);
    expect(container.textContent).toContain(nome);
    expect(container.innerHTML).toContain(CLICK);
    expect(container.innerHTML).not.toMatch(/\{\{.*?\}\}/);
  });

  it('logotipo-paródia em CSS (sem logo raster/cid) e conforma a marca "Microsft 365"', () => {
    const isca = templatesPredefinidos.find((t) => t.id === IDS.email)!;
    const { container } = render(
      <div dangerouslySetInnerHTML={{ __html: emailComProps('X', formatarDataAcessoBRT()) }} />,
    );
    expect(container.textContent).toContain('Microsft 365');
    expect(isca.corpoHtml).not.toContain('data:image/png');
    expect(isca.corpoHtml).not.toContain('cid:');
  });

  it('exibe a data no padrão do fuso de Brasília (America/Sao_Paulo → "(BRT)")', () => {
    const data = formatarDataAcessoBRT(new Date('2026-03-10T14:05:00'));
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: emailComProps('X', data) }} />);
    expect(container.textContent).toContain(data);
    expect(container.textContent).toMatch(/\d{2}\/\d{2}\/\d{4} às \d{2}:\d{2} \(BRT\)/);
  });
});

describe('Microsft 365 — Página Simulada (Phishing)', () => {
  function landingHtml() {
    const l = landingTemplates.find((x) => x.id === IDS.landing);
    if (!l) throw new Error('landing microcorp-login não encontrada');
    return l.html.replaceAll('{{CAMPAIGN_ID}}', CAMP).replaceAll('{{TARGET_ID}}', TGT);
  }

  it('renderiza o formulário de captura (e-mail + senha)', () => {
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: landingHtml() }} />);
    expect(container.querySelector('form')).toBeInTheDocument();
    expect(container.querySelector('#mc-email')).toBeInTheDocument();
    expect(container.querySelector('#mc-password')).toBeInTheDocument();
  });

  it('ao submeter: envia só metadados (LGPD) para /api/tracking/submit e redireciona ao treinamento', async () => {
    const fetchMock = vi.fn(() => Promise.resolve({ ok: true, json: async () => ({}) }));
    vi.stubGlobal('fetch', fetchMock);
    const loc: { href: string; search: string } = { href: '', search: '' };
    const orig = Object.getOwnPropertyDescriptor(window, 'location');
    Object.defineProperty(window, 'location', { configurable: true, value: loc });
    try {
      const { container } = render(<div dangerouslySetInnerHTML={{ __html: landingHtml() }} />);
      (container.querySelector('#mc-email') as HTMLInputElement).value = 'user@corp.com';
      (container.querySelector('#mc-password') as HTMLInputElement).value = 'corp1234';

      const form = container.querySelector('form') as HTMLFormElement;
      const onsubmit = form.getAttribute('onsubmit');
      expect(onsubmit).toBeTruthy();
      new Function('event', onsubmit as string).call(form, { preventDefault: vi.fn() });

      await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1));
      const [url, opts] = fetchMock.mock.calls[0] as unknown as [string, RequestInit];
      expect(url).toBe(`/api/tracking/submit/${CAMP}/${TGT}`);
      expect(opts.method).toBe('POST');
      const body = JSON.parse(opts.body as string);
      expect(body).toMatchObject({ camposPreenchidos: true, tamanhoSenha: 'corp1234'.length });
      // LGPD: sem e-mail/senha reais no payload.
      expect(opts.body as string).not.toContain('user@corp.com');
      expect(opts.body as string).not.toContain('corp1234');

      await waitFor(() =>
        expect(loc.href).toBe(`/educational-feedback?template=microsft365&c=${CAMP}&t=${TGT}`),
      );
    } finally {
      if (orig) Object.defineProperty(window, 'location', orig);
      vi.unstubAllGlobals();
    }
  });
});

describe('Microsft 365 — Tela Educacional (Just-in-Time)', () => {
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
    expect(screen.getByText(/Erro Sutil no Nome da Empresa/i)).toBeInTheDocument();
    expect(screen.getByText(/Falso Alerta de Expiração de Senha/i)).toBeInTheDocument();
    expect(screen.getByText(/Link Externo para Captura de Credenciais/i)).toBeInTheDocument();
    expect(screen.getByText(/Remetente Automático e Sem Assinatura Oficial/i)).toBeInTheDocument();
    expect(screen.getByText(/Portal Falso de Captura de Senha/i)).toBeInTheDocument();
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
