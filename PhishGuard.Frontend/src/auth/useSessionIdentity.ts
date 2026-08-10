import { useMemo, useSyncExternalStore } from 'react';
import {
  getSessionIdentity,
  getToken,
  subscribeSession,
  type SessionIdentity,
} from './session';

const EMPTY_IDENTITY: SessionIdentity = { name: '', email: '' };

/** Identidade reativa da sessão atual, atualizada sem recarregar a página. */
export function useSessionIdentity(): SessionIdentity {
  const token = useSyncExternalStore(subscribeSession, getToken, () => null);
  return useMemo(() => getSessionIdentity(token) ?? EMPTY_IDENTITY, [token]);
}
