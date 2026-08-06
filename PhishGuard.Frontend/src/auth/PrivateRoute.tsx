import { type ReactNode, useEffect, useState } from 'react';
import { Navigate } from 'react-router-dom';
import { restoreSession, validateSession } from './session';

/**
 * Barreira de acesso ao painel administrativo.
 *
 * Antes, a checagem era só "existe token no localStorage?" — um token residual de
 * OUTRA conta (ou expirado, ou sem escopo de tenant) abria o painel, e cada tela
 * requisitava a API com ele: os dados renderizados eram do tenant do token velho,
 * não da conta que o usuário acabou de criar (falha de isolamento de sessão).
 *
 * Agora a sessão passa por `validateSession()` (claims `tenant_id` e `exp`). Sessão
 * reprovada é REVOGADA na hora (`clearSession()`) — não fica um token podre no
 * storage esperando a próxima navegação — e o usuário é enviado ao login limpo.
 */
export default function PrivateRoute({ children }: { children: ReactNode }) {
  const [state, setState] = useState<'checking' | 'authenticated' | 'anonymous'>(
    () => validateSession().valida ? 'authenticated' : 'checking',
  );

  useEffect(() => {
    if (state !== 'checking') return;
    let active = true;
    void restoreSession().then((restored) => {
      if (active) setState(restored ? 'authenticated' : 'anonymous');
    });
    return () => { active = false; };
  }, [state]);

  if (state === 'checking') return null;
  if (state === 'anonymous') return <Navigate to="/login" replace />;
  return <>{children}</>;
}
