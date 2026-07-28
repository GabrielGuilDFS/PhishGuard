import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import type {
  FeedbackMockup,
  FeedbackTrainingConfig,
} from '../data/feedbackTrainings';
import {
  TRACKING_ACTIONS,
  readEducationalFeedbackParams,
  trackingEndpoint,
} from '../shared/trackingContract';

// ============================================================================
// FeedbackTraining — Tela Educacional de Feedback (Just-in-Time Training)
// ============================================================================
//
// Componente 100% GENÉRICO e REUTILIZÁVEL: toda a informação específica da isca
// (textos, hotspots, cards, mockups) vem do objeto `config` (ver
// data/feedbackTrainings.ts). O MESMO componente atende amzprime, bho MAX e
// NetsFlix — basta trocar a config.
//
// Estilo: Tailwind CSS (utilitários), coerente com as landing pages. Sem MUI
// nesta rota pública. As iscas são renderizadas em <iframe sandbox> (somente
// leitura, sem scripts) — nenhum HTML de origem do usuário executa aqui.

interface Props {
  config: FeedbackTrainingConfig;
  /**
   * Modo PREVIEW (Biblioteca de Modelos): renderiza a tela embutida no painel
   * administrativo, sem ocupar a viewport inteira e SEM navegar o roteador do
   * admin ao concluir (o botão vira apenas ilustrativo). Fora do preview
   * (rota /educational-feedback real), a tela é a página completa do alvo.
   */
  preview?: boolean;
}

// ---------------------------------------------------------------------------
// Painel de mockup: renderiza a isca num iframe escalado + badges de hotspot.
// ---------------------------------------------------------------------------
function MockupPanel({
  mockup,
  ativo,
  onHotspot,
}: {
  mockup: FeedbackMockup;
  ativo: number | null;
  onHotspot: (numero: number) => void;
}) {
  const wrapperRef = useRef<HTMLDivElement>(null);
  const [escala, setEscala] = useState(1);

  // Escala o documento lógico para caber na largura do painel; a altura visível
  // fica fixa (mockup.altura) e mostramos a fatia superior da isca.
  useLayoutEffect(() => {
    const el = wrapperRef.current;
    if (!el) return;
    const recalcular = () => setEscala(el.clientWidth / mockup.larguraLogica);
    recalcular();
    const ro = new ResizeObserver(recalcular);
    ro.observe(el);
    return () => ro.disconnect();
  }, [mockup.larguraLogica]);

  return (
    <div className="flex flex-col overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      {/* Cromo de contexto */}
      {mockup.chrome.tipo === 'email' ? (
        <div className="flex items-center gap-3 border-b border-slate-200 bg-slate-50 px-4 py-3">
          <div className="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full bg-slate-300 text-sm font-bold text-slate-600">
            ?
          </div>
          <div className="min-w-0 leading-tight">
            <div className="truncate text-sm font-semibold text-slate-800">
              {mockup.chrome.remetenteNome}{' '}
              <span className="font-normal text-slate-500">
                &lt;{mockup.chrome.remetenteEmail}&gt;
              </span>
            </div>
            <div className="truncate text-xs text-slate-500">{mockup.chrome.assunto}</div>
          </div>
        </div>
      ) : (
        <div className="flex items-center gap-2 border-b border-slate-200 bg-slate-50 px-3 py-2">
          <span className="flex gap-1.5">
            <span className="h-3 w-3 rounded-full bg-red-400" />
            <span className="h-3 w-3 rounded-full bg-yellow-400" />
            <span className="h-3 w-3 rounded-full bg-green-400" />
          </span>
          <div className="flex flex-1 items-center gap-2 rounded-md border border-slate-200 bg-white px-3 py-1 text-xs text-slate-600">
            <span className={mockup.chrome.seguro ? 'text-green-600' : 'text-red-500'}>
              {mockup.chrome.seguro ? '🔒' : '⚠️'}
            </span>
            <span className="truncate font-mono">{mockup.chrome.url}</span>
          </div>
        </div>
      )}

      {/* Área do mockup: iframe escalado + hotspots absolutos */}
      <div className="relative bg-white" style={{ height: mockup.altura }} ref={wrapperRef}>
        <iframe
          title={mockup.titulo}
          sandbox=""
          scrolling="no"
          aria-hidden="true"
          tabIndex={-1}
          srcDoc={mockup.html}
          style={{
            width: mockup.larguraLogica,
            height: escala > 0 ? mockup.altura / escala : mockup.altura,
            border: 0,
            transform: `scale(${escala})`,
            transformOrigin: 'top left',
            pointerEvents: 'none',
          }}
        />

        {mockup.hotspots.map((h) => {
          const isAtivo = ativo === h.numero;
          return (
            <button
              key={h.numero}
              type="button"
              title={h.dica}
              aria-label={`Ponto de atenção ${h.numero}: ${h.dica}`}
              onClick={() => onHotspot(h.numero)}
              className={[
                'absolute flex h-8 w-8 -translate-x-1/2 -translate-y-1/2 cursor-pointer items-center justify-center',
                'rounded-full border-2 border-white text-sm font-bold text-white shadow-lg transition-transform',
                'hover:scale-110 focus:outline-none focus:ring-2 focus:ring-offset-2',
                isAtivo ? 'bg-red-600 ring-2 ring-red-300 focus:ring-red-500' : 'bg-red-500 focus:ring-red-400',
              ].join(' ')}
              style={{
                left: `${h.xPct}%`,
                top: `${h.yPct}%`,
                animation: isAtivo ? undefined : 'fbHotspotPulse 2s infinite',
              }}
            >
              {h.numero}
            </button>
          );
        })}

        <span className="pointer-events-none absolute bottom-2 left-1/2 -translate-x-1/2 rounded-full bg-slate-900/70 px-3 py-1 text-[11px] font-medium text-white">
          {mockup.titulo}
        </span>
      </div>
    </div>
  );
}

export default function FeedbackTraining({ config, preview = false }: Props) {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [ativo, setAtivo] = useState<number | null>(null);
  const [concluindo, setConcluindo] = useState(false);
  const cardRefs = useRef<Record<number, HTMLDivElement | null>>({});

  // IDs de rastreamento vindos da landing (best-effort — podem não existir num
  // acesso direto à rota). Usados só para AUDITAR a conclusão do módulo. Os nomes
  // dos parâmetros (c/t) vêm do contrato compartilhado — nada de fallback legado
  // para 'campaign'/'target', que era justamente a divergência §1.3d eliminada.
  const { campaignId: campaignParam, targetId: targetParam } =
    readEducationalFeedbackParams(searchParams);
  const campaignId = campaignParam || '';
  const targetId = targetParam || '';

  // Clique num hotspot destaca e rola até o card explicativo correspondente.
  const focarCard = useCallback((numero: number) => {
    setAtivo(numero);
    cardRefs.current[numero]?.scrollIntoView({ behavior: 'smooth', block: 'center' });
  }, []);

  const concluir = useCallback(async () => {
    // No preview (Biblioteca de Modelos) o botão é ilustrativo: não audita nem
    // navega, para não sequestrar o roteador do painel administrativo.
    if (preview) return;
    setConcluindo(true);
    // Registra a conclusão para fins de AUDITORIA do módulo (best-effort: uma
    // falha de rede não pode prender o usuário na tela). Só dispara se houver
    // contexto de campanha/alvo.
    if (campaignId && targetId) {
      try {
        await fetch(trackingEndpoint(TRACKING_ACTIONS.complete, campaignId, targetId), {
          method: 'POST',
          headers: { 'ngrok-skip-browser-warning': 'true' },
        });
      } catch {
        /* silencioso: a conclusão não deve travar por erro de telemetria */
      }
    }
    navigate(config.conclusao.redirecionarPara);
  }, [preview, campaignId, targetId, navigate, config.conclusao.redirecionarPara]);

  // Keyframes do pulso do hotspot (injetado uma vez; sem preflight do Tailwind
  // não há @keyframes global, então declaramos aqui de forma auto-contida).
  useEffect(() => {
    const styleId = 'fb-hotspot-keyframes';
    if (document.getElementById(styleId)) return;
    const style = document.createElement('style');
    style.id = styleId;
    style.textContent =
      '@keyframes fbHotspotPulse{0%{box-shadow:0 0 0 0 rgba(239,68,68,.6)}70%{box-shadow:0 0 0 10px rgba(239,68,68,0)}100%{box-shadow:0 0 0 0 rgba(239,68,68,0)}}';
    document.head.appendChild(style);
  }, []);

  return (
    <div
      className={[
        'bg-slate-100 px-4 text-slate-800',
        // Página completa (rota do alvo) ocupa a viewport; no preview embutido no
        // painel, apenas preenche o container do diálogo, sem forçar 100vh.
        preview ? 'min-h-full py-6' : 'min-h-screen py-8',
      ].join(' ')}
    >
      <div className="mx-auto max-w-6xl">
        {/* 1. HEADER DE FEEDBACK */}
        <header className="mb-6 overflow-hidden rounded-2xl bg-white shadow-md">
          <div className="flex items-start gap-3 bg-amber-400 px-6 py-4">
            <span className="text-2xl leading-none" aria-hidden="true">⚠️</span>
            <p className="text-sm font-bold text-amber-950 sm:text-base">{config.header.badge}</p>
          </div>
          <div className="px-6 py-6">
            <h1 className="text-2xl font-extrabold text-slate-900 sm:text-3xl">
              {config.header.titulo}
            </h1>
            <p className="mt-3 max-w-3xl text-base leading-relaxed text-slate-600">
              {config.header.mensagem}
            </p>
          </div>
        </header>

        {/* 2. VISUALIZAÇÃO INTERATIVA DA ISCA */}
        <section className="mb-8">
          <h2 className="mb-1 text-lg font-bold text-slate-900">
            Onde estavam os sinais de alerta?
          </h2>
          <p className="mb-4 text-sm text-slate-500">
            Clique nos números destacados para entender cada indicador de risco.
          </p>
          <div className="grid gap-6 lg:grid-cols-2">
            {config.mockups.map((m) => (
              <MockupPanel key={m.id} mockup={m} ativo={ativo} onHotspot={focarCard} />
            ))}
          </div>
        </section>

        {/* 3. SEÇÃO DE EXPLICAÇÃO DETALHADA */}
        <section className="mb-8">
          <h2 className="mb-4 text-lg font-bold text-slate-900">Indicadores de risco</h2>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {config.cards.map((card) => {
              const isAtivo = ativo === card.numero;
              return (
                <div
                  key={card.numero}
                  ref={(el) => {
                    cardRefs.current[card.numero] = el;
                  }}
                  onMouseEnter={() => setAtivo(card.numero)}
                  className={[
                    'rounded-xl border bg-white p-5 shadow-sm transition-all',
                    isAtivo
                      ? 'border-red-400 ring-2 ring-red-200'
                      : 'border-slate-200 hover:border-slate-300',
                  ].join(' ')}
                >
                  <div className="mb-3 flex items-center gap-3">
                    <span className="flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full bg-red-500 text-sm font-bold text-white">
                      {card.numero}
                    </span>
                    <span className="text-2xl leading-none" aria-hidden="true">{card.icone}</span>
                    <h3 className="text-base font-bold text-slate-900">{card.titulo}</h3>
                  </div>
                  <ul className="space-y-2 pl-1">
                    {card.pontos.map((p, i) => (
                      <li key={i} className="flex gap-2 text-sm leading-snug text-slate-600">
                        <span className="mt-0.5 flex-shrink-0 text-red-400" aria-hidden="true">•</span>
                        <span>{p}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              );
            })}
          </div>
        </section>

        {/* 4. BOTÃO DE CONCLUSÃO */}
        <div className="flex flex-col items-center gap-3 border-t border-slate-200 pt-8">
          <button
            type="button"
            onClick={concluir}
            disabled={concluindo}
            className="inline-flex items-center gap-2 rounded-lg bg-emerald-600 px-8 py-3 text-base font-bold text-white shadow-md transition-colors hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-400 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-70"
          >
            {concluindo ? (
              <>
                <span className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                Registrando conclusão…
              </>
            ) : (
              <>✓ {config.conclusao.label}</>
            )}
          </button>
          <p className="text-xs text-slate-400">
            {preview
              ? 'Pré-visualização — no fluxo real, concluir registra a participação para auditoria do módulo.'
              : 'Ao concluir, sua participação é registrada para fins de auditoria do módulo.'}
          </p>
        </div>
      </div>
    </div>
  );
}
