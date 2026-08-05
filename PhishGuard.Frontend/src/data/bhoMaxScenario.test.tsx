import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { templatesPredefinidos } from './predefinedTemplates';
import { landingTemplates } from './landingTemplates';
import { feedbackTrainings } from './feedbackTrainings';
import { formatarExpiracaoLink } from '../utils/emailExpiration';
import FeedbackTraining from '../components/FeedbackTraining';

// ============================================================================
// Suíte do CENÁRIO "bho MAX" — mesmo BLUEPRINT de mercadoLivScenario.test.tsx,
// adaptado à estrutura REAL desta isca:
//   • E-mail (hbomax-redefinicao-senha): logo em PNG (data-URI no preview /
//     cid:logo-bhomax no disparo); placeholders {{LINK_PHISHING}} + {{DATA_EXPIRACAO}}.
//   • Página falsa (hbomax-redefinicao-senha): captura "senha atual" + "nova senha".
//   • Tela educacional (feedbackTrainings.bhomax).
// Vitest + RTL (jsdom); fetch/useNavigate mockados e isolados por arquivo.
// ============================================================================

const { navigateMock } = vi.hoisted(() => ({ navigateMock: vi.fn() }));
vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-router-dom')>();
  return { ...actual, useNavigate: () => navigateMock };
});

const IDS = { email: 'hbomax-redefinicao-senha', landing: 'hbomax-redefinicao-senha', feedback: 'bhomax' } as const;
const CAMP = 'camp-1';
const TGT = 'tgt-1';
const CLICK = 'https://phishguard.example/api/tracking/click/camp-1/tgt-1';

// ----------------------------------------------------------------------------
// 1) TEMPLATE DE E-MAIL
// ----------------------------------------------------------------------------
describe('bho MAX — Template de E-mail', () => {
  function emailComProps() {
    const isca = templatesPredefinidos.find((t) => t.id === IDS.email);
    if (!isca) throw new Error('isca hbomax não encontrada');
    return isca.corpoHtml
      .replaceAll('{{LINK_PHISHING}}', CLICK)
      .replaceAll('{{DATA_EXPIRACAO}}', formatarExpiracaoLink(new Date('2026-01-15T09:30:00')));
  }

  it('renderiza com placeholders substituídos (link/data) e sem placeholders remanescentes', () => {
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: emailComProps() }} />);
    expect(container.innerHTML).toContain(CLICK);
    expect(container.innerHTML).not.toMatch(/\{\{.*?\}\}/);
  });

  it('injeta a logo própria como PNG (data-URI no preview; cid:logo-bhomax no disparo) e conforma a marca', () => {
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: emailComProps() }} />);
    const temPng = Array.from(container.querySelectorAll('img')).some((i) =>
      i.getAttribute('src')?.startsWith('data:image/png'),
    );
    expect(temPng).toBe(true);
    // Conformidade da marca exibida no corpo.
    expect(container.textContent).toContain('bho MAX');
  });

  it('data de expiração no formato dinâmico do template', () => {
    const data = formatarExpiracaoLink(new Date('2026-03-10T14:05:00'));
    const isca = templatesPredefinidos.find((t) => t.id === IDS.email)!;
    const html = isca.corpoHtml.replaceAll('{{LINK_PHISHING}}', '#').replaceAll('{{DATA_EXPIRACAO}}', data);
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: html }} />);
    expect(container.textContent).toContain(data);
    expect(container.textContent).toMatch(/\w{3} \d{2}, \d{4} às \d{2}:\d{2} (AM|PM)/);
  });
});

// ----------------------------------------------------------------------------
// 2) PÁGINA SIMULADA (PHISHING)
// ----------------------------------------------------------------------------
describe('bho MAX — Página Simulada (Phishing)', () => {
  function landingHtml() {
    const l = landingTemplates.find((x) => x.id === IDS.landing);
    if (!l) throw new Error('landing hbomax não encontrada');
    return l.html.replaceAll('{{CAMPAIGN_ID}}', CAMP).replaceAll('{{TARGET_ID}}', TGT);
  }

  it('renderiza o formulário de captura (senha atual + nova senha)', () => {
    const { container } = render(<div dangerouslySetInnerHTML={{ __html: landingHtml() }} />);
    expect(container.querySelector('form')).toBeInTheDocument();
    expect(container.querySelector('#current-password')).toBeInTheDocument();
    expect(container.querySelector('#new-password')).toBeInTheDocument();
  });

  it('ao submeter: envia só metadados (LGPD) para /api/tracking/submit e redireciona ao treinamento', async () => {
    const fetchMock = vi.fn(() => Promise.resolve({ ok: true, json: async () => ({}) }));
    vi.stubGlobal('fetch', fetchMock);
    const loc: { href: string; search: string } = { href: '', search: '' };
    const orig = Object.getOwnPropertyDescriptor(window, 'location');
    Object.defineProperty(window, 'location', { configurable: true, value: loc });
    try {
      const { container } = render(<div dangerouslySetInnerHTML={{ __html: landingHtml() }} />);
      (container.querySelector('#current-password') as HTMLInputElement).value = 'atual987';
      (container.querySelector('#new-password') as HTMLInputElement).value = 'NovaSenha123';

      const form = container.querySelector('form') as HTMLFormElement;
      const onsubmit = form.getAttribute('onsubmit');
      expect(onsubmit).toBeTruthy();
      new Function('event', onsubmit as string).call(form, { preventDefault: vi.fn() });

      await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1));
      const [url, opts] = fetchMock.mock.calls[0] as unknown as [string, RequestInit];
      expect(url).toBe(`/api/tracking/submit/${CAMP}/${TGT}`);
      expect(opts.method).toBe('POST');
      const body = JSON.parse(opts.body as string);
      expect(body).toMatchObject({ camposPreenchidos: true, tamanhoSenha: 'NovaSenha123'.length });
      // LGPD: nenhuma senha em texto no payload.
      expect(opts.body as string).not.toContain('NovaSenha123');
      expect(opts.body as string).not.toContain('atual987');

      await waitFor(() =>
        expect(loc.href).toBe(`/educational-feedback?template=bhomax&c=${CAMP}&t=${TGT}`),
      );
    } finally {
      if (orig) Object.defineProperty(window, 'location', orig);
      vi.unstubAllGlobals();
    }
  });
});

// ----------------------------------------------------------------------------
// 3) TELA EDUCACIONAL (JUST-IN-TIME)
// ----------------------------------------------------------------------------
describe('bho MAX — Tela Educacional (Just-in-Time)', () => {
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
    expect(screen.getByText(/Inversão do Nome da Marca/i)).toBeInTheDocument();
    expect(screen.getByText(/Pressão de Tempo e Padrão Estrangeiro/i)).toBeInTheDocument();
    expect(screen.getByText(/Captura da Credencial Ativa/i)).toBeInTheDocument();
    expect(screen.getByText(/Elementos Estáticos e Ausência do Seu E-mail/i)).toBeInTheDocument();
    expect(screen.getByText(/Domínio do Remetente Falsificado/i)).toBeInTheDocument();
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
