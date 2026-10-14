import { useEffect, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import FeedbackTraining from '../components/FeedbackTraining';
import { educationalTemplates } from '../data/educationalTemplates';
import { feedbackTrainings, type FeedbackTrainingConfig } from '../data/feedbackTrainings';
import { API_BASE } from '../config';
import { useNotify } from '../context/NotificationContext';
import { EDU_FEEDBACK_QUERY, TRACKING_ACTIONS } from '../shared/trackingContract';
import { sendOrQueueTrackingEvent } from '../shared/trackingRetryQueue';

interface EducationalContent {
  conteudoHtml: string;
  feedbackConfigJson?: FeedbackTrainingConfig | null;
}

function EducationalHtml({ html, campaignId, targetId, trackingToken }: {
  html: string;
  campaignId: string;
  targetId: string;
  trackingToken: string;
}) {
  const [concluindo, setConcluindo] = useState(false);
  const [concluido, setConcluido] = useState(false);
  const { showNotify } = useNotify();
  const tracked = Boolean(campaignId && targetId && trackingToken);

  useEffect(() => {
    if (!tracked) return;
    void sendOrQueueTrackingEvent(TRACKING_ACTIONS.educationalView, campaignId, targetId, trackingToken);
  }, [tracked, campaignId, targetId, trackingToken]);

  const concluir = async () => {
    setConcluindo(true);
    const viewResult = await sendOrQueueTrackingEvent(TRACKING_ACTIONS.educationalView, campaignId, targetId, trackingToken);
    const completeResult = await sendOrQueueTrackingEvent(TRACKING_ACTIONS.complete, campaignId, targetId, trackingToken);
    if (viewResult === 'queued' || completeResult === 'queued')
      showNotify('Treinamento concluído. O registro será reenviado quando a conexão estiver disponível.', 'warning');
    setConcluindo(false);
    setConcluido(true);
  };

  return (
    <main className="min-h-screen bg-slate-100 p-4 sm:p-8">
      <div className="mx-auto max-w-6xl overflow-hidden rounded-xl bg-white shadow-sm">
        <iframe title="Treinamento educativo da campanha" sandbox="" srcDoc={html}
          style={{ display: 'block', width: '100%', minHeight: '75vh', border: 0 }} />
        {tracked && (
          <div className="border-t border-slate-200 p-6 text-center">
            <button type="button" onClick={() => { void concluir(); }} disabled={concluindo || concluido}
              className="rounded-lg bg-blue-700 px-6 py-3 font-semibold text-white disabled:opacity-60">
              {concluido ? 'Treinamento concluído' : concluindo ? 'Registrando conclusão...' : 'Concluir treinamento'}
            </button>
          </div>
        )}
      </div>
    </main>
  );
}

export default function EducationalFeedbackPage() {
  const [searchParams] = useSearchParams();
  const templateId = searchParams.get(EDU_FEEDBACK_QUERY.template) || 'basico_phishing';
  const campaignId = searchParams.get(EDU_FEEDBACK_QUERY.campaign) || '';
  const targetId = searchParams.get(EDU_FEEDBACK_QUERY.target) || '';
  const trackingToken = searchParams.get(EDU_FEEDBACK_QUERY.token) || '';
  const tracked = Boolean(campaignId || targetId || trackingToken);
  const [content, setContent] = useState<EducationalContent | null>(null);
  const [failed, setFailed] = useState(false);
  const { showNotify } = useNotify();
  const notifyRef = useRef(showNotify);
  useEffect(() => { notifyRef.current = showNotify; }, [showNotify]);

  useEffect(() => {
    setContent(null);
    setFailed(false);
    if (!tracked) return;
    const controller = new AbortController();
    const query = new URLSearchParams({ t: targetId, k: trackingToken });
    void fetch(`${API_BASE}/tracking/educational/${campaignId}?${query}`, {
      signal: controller.signal,
      headers: { 'ngrok-skip-browser-warning': 'true' },
    }).then(async (response) => {
      if (!response.ok) throw new Error('Conteúdo indisponível');
      const result = await response.json() as EducationalContent;
      if (typeof result.conteudoHtml !== 'string') throw new Error('Conteúdo inválido');
      setContent(result);
    }).catch((error: unknown) => {
      if (error instanceof DOMException && error.name === 'AbortError') return;
      setFailed(true);
      notifyRef.current('Não foi possível carregar o treinamento desta campanha.', 'error');
    });
    return () => controller.abort();
  }, [tracked, campaignId, targetId, trackingToken]);

  if (tracked) {
    if (failed) return <p role="alert" className="p-8 text-center">Treinamento não encontrado ou link expirado.</p>;
    if (!content) return <p role="status" className="p-8 text-center">Carregando treinamento...</p>;
    if (content.feedbackConfigJson) return <FeedbackTraining key={campaignId} config={content.feedbackConfigJson} />;
    return <EducationalHtml key={campaignId} html={content.conteudoHtml} campaignId={campaignId} targetId={targetId} trackingToken={trackingToken} />;
  }

  const training = feedbackTrainings[templateId];
  if (training) return <FeedbackTraining config={training} />;

  const template = educationalTemplates.find((item) => item.id === templateId);
  const html = template?.html
    ?? '<div style="padding:2rem;text-align:center;font-family:sans-serif;color:#b00;">Treinamento não encontrado.</div>';
  return <EducationalHtml html={html} campaignId="" targetId="" trackingToken="" />;
}
