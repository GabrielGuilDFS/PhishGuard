import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { NotificationProvider } from '../context/NotificationContext';
import { feedbackTrainings } from '../data/feedbackTrainings';
import EducationalFeedbackPage from './EducationalFeedbackPage';

const fetchMock = vi.fn<typeof fetch>();
const entry = '/educational-feedback?template=netsflix&c=campaign-1&t=target-1&k=token-assinado';

function renderPage(initialEntry = entry) {
  return render(<NotificationProvider><MemoryRouter initialEntries={[initialEntry]}>
    <EducationalFeedbackPage />
  </MemoryRouter></NotificationProvider>);
}

function response(body: unknown, status = 200) {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
}

beforeEach(() => {
  fetchMock.mockReset();
  vi.stubGlobal('fetch', fetchMock);
  localStorage.clear();
});
afterEach(() => vi.unstubAllGlobals());

describe('EducationalFeedbackPage — conteúdo histórico', () => {
  it('usa a configuração preservada da campanha em vez do template indicado na URL', async () => {
    fetchMock.mockImplementation(async (url) => String(url).includes('/educational/')
      ? response({ conteudoHtml: '<div data-feedback-training="amzprime"></div>', feedbackConfigJson: {
        ...feedbackTrainings.amzprime,
        header: { ...feedbackTrainings.amzprime.header, titulo: 'Título preservado na ativação' },
      } }) : response({ status: 'ok' }));
    renderPage();
    expect(await screen.findByRole('heading', { name: 'Título preservado na ativação' })).toBeInTheDocument();
    const contentRequest = fetchMock.mock.calls.find(([url]) => String(url).includes('/educational/'));
    const url = new URL(String(contentRequest?.[0]), window.location.origin);
    expect(url.pathname).toBe('/api/tracking/educational/campaign-1');
    expect(url.searchParams.get('t')).toBe('target-1');
    expect(url.searchParams.get('k')).toBe('token-assinado');
    await waitFor(() => expect(fetchMock.mock.calls.some(([url]) => String(url).includes('/educational-view/'))).toBe(true));
  });

  it('isola HTML preservado e registra a conclusão do treinamento estático', async () => {
    fetchMock.mockImplementation(async (url) => String(url).includes('/educational/')
      ? response({ conteudoHtml: '<h1>Conteúdo histórico estático</h1>' }) : response({ status: 'ok' }));
    renderPage();
    const frame = await screen.findByTitle('Treinamento educativo da campanha');
    expect(frame).toHaveAttribute('srcdoc', '<h1>Conteúdo histórico estático</h1>');
    expect(frame).toHaveAttribute('sandbox', '');
    fireEvent.click(screen.getByRole('button', { name: 'Concluir treinamento' }));
    expect(await screen.findByRole('button', { name: 'Treinamento concluído' })).toBeDisabled();
    expect(fetchMock.mock.calls.some(([url, init]) => String(url).includes('/complete/') && init?.method === 'POST')).toBe(true);
  });

  it('não substitui uma campanha inválida pelo catálogo atual', async () => {
    fetchMock.mockResolvedValue(response({ message: 'Expirado' }, 404));
    renderPage();
    expect(await screen.findByText('Treinamento não encontrado ou link expirado.')).toBeInTheDocument();
    expect(screen.queryByRole('heading', { name: feedbackTrainings.netsflix.header.titulo })).not.toBeInTheDocument();
    expect(screen.getByText('Não foi possível carregar o treinamento desta campanha.')).toBeInTheDocument();
  });

  it('preserva acesso direto ao catálogo sem dados de rastreamento', () => {
    renderPage('/educational-feedback?template=amzprime');
    expect(screen.getByRole('heading', { name: feedbackTrainings.amzprime.header.titulo })).toBeInTheDocument();
    expect(fetchMock).not.toHaveBeenCalled();
  });
});
