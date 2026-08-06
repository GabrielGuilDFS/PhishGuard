import { TRACKING_ACTIONS, trackingEndpoint } from './trackingContract';

export const TRACKING_RETRY_STORAGE_KEY = 'phishguard_tracking_retry_v1';

const MAX_PENDING_EVENTS = 20;
const MAX_ATTEMPTS = 5;
const EVENT_TTL_MS = 7 * 24 * 60 * 60 * 1000;

interface PendingTrackingEvent {
  id: string;
  action: RetryableTrackingAction;
  campaignId: string;
  targetId: string;
  trackingToken: string;
  createdAt: number;
  attempts: number;
}

let retryInFlight: Promise<void> | null = null;
type RetryableTrackingAction =
  | typeof TRACKING_ACTIONS.educationalView
  | typeof TRACKING_ACTIONS.complete;

function isRetryableAction(value: unknown): value is RetryableTrackingAction {
  return value === TRACKING_ACTIONS.educationalView || value === TRACKING_ACTIONS.complete;
}

function eventId(action: RetryableTrackingAction, campaignId: string, targetId: string) {
  return `${action}:${campaignId}:${targetId}`;
}

function readQueue(): PendingTrackingEvent[] {
  try {
    const raw = window.localStorage.getItem(TRACKING_RETRY_STORAGE_KEY);
    if (!raw) return [];
    const parsed: unknown = JSON.parse(raw);
    if (!Array.isArray(parsed)) return [];

    const now = Date.now();
    return parsed.filter((item): item is PendingTrackingEvent => {
      if (!item || typeof item !== 'object') return false;
      const value = item as Partial<PendingTrackingEvent>;
      return typeof value.id === 'string'
        && isRetryableAction(value.action)
        && typeof value.campaignId === 'string'
        && typeof value.targetId === 'string'
        && typeof value.trackingToken === 'string'
        && typeof value.createdAt === 'number'
        && typeof value.attempts === 'number'
        && now - value.createdAt <= EVENT_TTL_MS
        && value.attempts < MAX_ATTEMPTS;
    });
  } catch {
    return [];
  }
}

function writeQueue(events: PendingTrackingEvent[]) {
  try {
    if (events.length === 0) {
      window.localStorage.removeItem(TRACKING_RETRY_STORAGE_KEY);
      return;
    }
    window.localStorage.setItem(
      TRACKING_RETRY_STORAGE_KEY,
      JSON.stringify(events.slice(-MAX_PENDING_EVENTS)),
    );
  } catch {
    // Storage indisponível (modo privado/política do navegador): a UX educacional
    // continua, embora não seja possível persistir o retry desta telemetria.
  }
}

async function postTrackingEvent(event: Pick<PendingTrackingEvent, 'action' | 'campaignId' | 'targetId' | 'trackingToken'>) {
  const response = await fetch(
    trackingEndpoint(event.action, event.campaignId, event.targetId, event.trackingToken),
    {
      method: 'POST',
      keepalive: true,
      headers: { 'ngrok-skip-browser-warning': 'true' },
    },
  );

  if (!response.ok) {
    throw new Error(`Tracking respondeu HTTP ${response.status}.`);
  }
}

function enqueue(event: Omit<PendingTrackingEvent, 'id' | 'createdAt' | 'attempts'>) {
  const id = eventId(event.action, event.campaignId, event.targetId);
  const current = readQueue();
  if (current.some((item) => item.id === id)) return;

  writeQueue([
    ...current,
    { ...event, id, createdAt: Date.now(), attempts: 0 },
  ]);
}

export async function sendOrQueueTrackingEvent(
  action: RetryableTrackingAction,
  campaignId: string,
  targetId: string,
  trackingToken: string,
): Promise<'sent' | 'queued'> {
  try {
    await postTrackingEvent({ action, campaignId, targetId, trackingToken });
    return 'sent';
  } catch {
    enqueue({ action, campaignId, targetId, trackingToken });
    return 'queued';
  }
}

export function retryPendingTrackingEvents(): Promise<void> {
  if (retryInFlight) return retryInFlight;

  retryInFlight = (async () => {
    const snapshot = readQueue();
    if (snapshot.length === 0) return;

    const retained: PendingTrackingEvent[] = [];
    for (const event of snapshot) {
      try {
        await postTrackingEvent(event);
      } catch {
        const next = { ...event, attempts: event.attempts + 1 };
        if (next.attempts < MAX_ATTEMPTS) retained.push(next);
      }
    }

    // Preserva eventos que possam ter sido enfileirados enquanto o retry rodava.
    const processedIds = new Set(snapshot.map((event) => event.id));
    const newlyQueued = readQueue().filter((event) => !processedIds.has(event.id));
    writeQueue([...retained, ...newlyQueued]);
  })().finally(() => {
    retryInFlight = null;
  });

  return retryInFlight;
}
