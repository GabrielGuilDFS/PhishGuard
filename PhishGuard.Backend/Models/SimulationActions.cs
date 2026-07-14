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
        public const string Abertura = "Abertura";
        public const string Clique = "Clique";
        public const string Submissao = "Submissão de Dados";
    }
}
