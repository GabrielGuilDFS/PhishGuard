import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { TRACKING_ACTIONS } from './trackingContract';
import {
  TRACKING_RETRY_STORAGE_KEY,
  retryPendingTrackingEvents,
  sendOrQueueTrackingEvent,
} from './trackingRetryQueue';

describe('trackingRetryQueue', () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    localStorage.clear();
    fetchMock.mockReset();
    vi.stubGlobal('fetch', fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    localStorage.clear();
  });

  it('envia a conclusão com keepalive sem persistir dados locais quando a API responde sucesso', async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 200 }));

    const result = await sendOrQueueTrackingEvent(TRACKING_ACTIONS.complete, 'camp-1', 'target-1', 'signed');

    expect(result).toBe('sent');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/tracking/complete/camp-1/target-1?k=signed',
      expect.objectContaining({ method: 'POST', keepalive: true }),
    );
    expect(localStorage.getItem(TRACKING_RETRY_STORAGE_KEY)).toBeNull();
  });

  it('enfileira resposta 500 sem dados sensíveis e remove o evento após retry bem-sucedido', async () => {
    fetchMock.mockResolvedValueOnce(new Response(null, { status: 500 }));

    const result = await sendOrQueueTrackingEvent(TRACKING_ACTIONS.complete, 'camp-2', 'target-2', 'signed');
    const stored = localStorage.getItem(TRACKING_RETRY_STORAGE_KEY);

    expect(result).toBe('queued');
    expect(stored).toContain('camp-2');
    expect(stored).toContain('target-2');
    expect(stored).not.toMatch(/email|senha|password/i);

    fetchMock.mockResolvedValueOnce(new Response(null, { status: 200 }));
    await retryPendingTrackingEvents();

    expect(localStorage.getItem(TRACKING_RETRY_STORAGE_KEY)).toBeNull();
  });

  it('deduplica a fila local quando a mesma conclusão falha mais de uma vez', async () => {
    fetchMock.mockRejectedValue(new TypeError('offline'));

    await sendOrQueueTrackingEvent(TRACKING_ACTIONS.complete, 'camp-3', 'target-3', 'signed');
    await sendOrQueueTrackingEvent(TRACKING_ACTIONS.complete, 'camp-3', 'target-3', 'signed');

    const queue = JSON.parse(localStorage.getItem(TRACKING_RETRY_STORAGE_KEY) ?? '[]') as unknown[];
    expect(queue).toHaveLength(1);
  });

  it('também mantém o acesso educacional para retry sem confundi-lo com a conclusão', async () => {
    fetchMock.mockRejectedValue(new TypeError('offline'));

    await sendOrQueueTrackingEvent(TRACKING_ACTIONS.educationalView, 'camp-4', 'target-4', 'signed');
    await sendOrQueueTrackingEvent(TRACKING_ACTIONS.complete, 'camp-4', 'target-4', 'signed');

    const queue = JSON.parse(localStorage.getItem(TRACKING_RETRY_STORAGE_KEY) ?? '[]') as Array<{ action: string }>;
    expect(queue.map((event) => event.action)).toEqual(['educational-view', 'complete']);
  });
});
