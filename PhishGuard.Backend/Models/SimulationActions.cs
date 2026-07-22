namespace PhishGuard.Backend.Models
{
    /// <summary>
    /// Valores canônicos da coluna <c>Acao</c> de <see cref="SimulationLog"/>.
    /// <para>
    /// <c>Envio</c> é o marcador de IDEMPOTÊNCIA do disparo: um log de Envio para
    /// (CampaignId, TargetId) significa que o e-mail já saiu para aquele alvo. O
    /// serviço de disparo consulta esse marcador antes de enviar e pula quem já
    /// recebeu — garantindo que um restart no meio do lote não gere duplicatas.
    /// </para>
    /// Os demais valores são registrados (anonimamente) pelo TrackingController.
    /// </summary>
    public static class SimulationActions
    {
        public const string Envio = "Envio";

        /// <summary>
        /// Registrado quando o envio para um alvo específico falha DEFINITIVAMENTE (após
        /// esgotar as tentativas de retry SMTP). Diferente de <see cref="Envio"/>, NÃO
        /// conta como entrega e não bloqueia o processamento dos demais alvos do lote.
        /// </summary>
        public const string Falha = "Falha";

        public const string Abertura = "Abertura";
        public const string Clique = "Clique";
        public const string Submissao = "Submissão de Dados";

        /// <summary>
        /// Registrado quando o alvo finaliza a Tela Educacional de Feedback
        /// (Just-in-Time Training) clicando em "Concluir Treinamento". Serve de
        /// trilha de AUDITORIA de que o módulo de conscientização foi consumido.
        /// </summary>
        public const string TreinamentoConcluido = "Conclusão de Treinamento";
    }
}
