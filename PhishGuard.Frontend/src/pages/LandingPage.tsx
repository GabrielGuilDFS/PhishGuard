import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { landingTemplates } from '../data/landingTemplates';
import { API_BASE } from '../config';
import {
    TRACKING_ACTIONS,
    educationalFeedbackUrl,
    trackingEndpoint,
} from '../shared/trackingContract';

const TRAINING_BY_LANDING: Record<string, string> = {
    'hbomax-redefinicao-senha': 'bhomax',
    'netflix-login': 'netsflix',
    'amazon-login': 'amzprime',
    'microcorp-login': 'microsft365',
    'mercadoliv-login': 'mercadoliv',
};

export default function LandingPage() {
    const { id } = useParams(); // ID da PhishingPage salva no banco
    const [searchParams] = useSearchParams();
    const campaignId = searchParams.get('c') || '';
    const targetId = searchParams.get('t') || '';
    const trackingToken = searchParams.get('k') || '';

    const [htmlContent, setHtmlContent] = useState<string>(
        '<div style="padding: 2rem; text-align: center; font-family: sans-serif;">Carregando ambiente seguro...</div>'
    );
    const [landingTemplateId, setLandingTemplateId] = useState('');

    useEffect(() => {
        if (!id) return;

        // Busca o HTML da página falsa cadastrada no banco de dados.
        // Em dev usa /api pelo proxy Vite; no Static Site usa a URL pública da API.
        fetch(`${API_BASE}/PhishingPages/${id}`, {
            // Pula a página de aviso do ngrok (free/estático) para o fetch receber JSON,
            // não o HTML interstitial. Inócuo fora do túnel.
            headers: { 'ngrok-skip-browser-warning': 'true' },
        })
            .then(res => {
                if (!res.ok) throw new Error('Página não encontrada');
                return res.json();
            })
            .then(data => {
                if (data && data.conteudoHtml) {
                    // Persistência simplificada: as armadilhas passam a guardar apenas o
                    // ID do molde oficial. Resolve o ID para o HTML do catálogo; se não
                    // casar, trata o valor como HTML bruto (compatibilidade com registros
                    // legados criados antes da refatoração do MVP).
                    const molde = landingTemplates.find((t) => t.id === data.conteudoHtml);
                    const rawHtml = molde ? molde.html : data.conteudoHtml;
                    setLandingTemplateId(molde?.id ?? '');

                    // Preenche os placeholders dinamicamente com os dados da vítima.
                    // {{CAMPAIGN_ID}} e {{TARGET_ID}} alimentam o gatilho de telemetria
                    // embutido no <form> dos moldes (ex.: HBO Max - Redefinição de Senha).
                    const processedHtml = rawHtml
                        // A submissão é tratada pelo React abaixo. Remover handlers inline
                        // permite uma CSP sem unsafe-inline para scripts e bloqueia HTML
                        // legado que tente introduzir novos eventos JavaScript.
                        .replace(/\son[a-z]+\s*=\s*"[^"]*"/gi, '')
                        .replace(/\son[a-z]+\s*=\s*'[^']*'/gi, '')
                        .replace(/{{CAMPAIGN_ID}}/g, campaignId)
                        .replace(/{{TARGET_ID}}/g, targetId)
                        .replace(/{{TRACKING_TOKEN}}/g, trackingToken);

                    setHtmlContent(processedHtml);
                }
            })
            .catch(() =>
                setHtmlContent('<div style="padding: 2rem; text-align: center; font-family: sans-serif; color: red;">Erro 404: Página não encontrada ou link expirado.</div>')
            );
    }, [id, campaignId, targetId, trackingToken]);

    const handleSubmit = useCallback((event: FormEvent<HTMLDivElement>) => {
        event.preventDefault();
        if (!campaignId || !targetId || !trackingToken) return;

        const form = event.target instanceof HTMLFormElement ? event.target : null;
        if (!form) return;
        const passwordValues = Array.from(form.querySelectorAll<HTMLInputElement>('input[type="password"]'))
            .map((input) => input.value);
        const emailValue = form.querySelector<HTMLInputElement>('input[type="email"]')?.value ?? '';
        const metadata = {
            camposPreenchidos: [...passwordValues, ...(emailValue ? [emailValue] : [])]
                .every((value) => value.length > 0) && passwordValues.length > 0,
            senhasCoincidem: passwordValues.length > 1
                ? passwordValues.every((value) => value === passwordValues[0] && value.length > 0)
                : false,
            tamanhoSenha: passwordValues[0]?.length ?? 0,
            emailInformado: emailValue.length > 0,
        };

        void fetch(trackingEndpoint(
            TRACKING_ACTIONS.submit,
            campaignId,
            targetId,
            trackingToken,
        ), {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'ngrok-skip-browser-warning': 'true',
            },
            body: JSON.stringify(metadata),
        }).finally(() => {
            window.location.href = educationalFeedbackUrl({
                template: TRAINING_BY_LANDING[landingTemplateId] ?? 'basico_phishing',
                campaignId,
                targetId,
                trackingToken,
            });
        });
    }, [campaignId, landingTemplateId, targetId, trackingToken]);

    // Renderiza o HTML injetando ele diretamente na DOM da página
    return <div onSubmitCapture={handleSubmit} dangerouslySetInnerHTML={{ __html: htmlContent }} />;
}
