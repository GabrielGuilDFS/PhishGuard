namespace PhishGuard.Backend.DTOs
{
    /// <summary>
    /// Resposta do <c>POST /api/tracking/submit/:c/:t</c>. Contrato explícito (não
    /// objeto anônimo) para que front e testes dependam de um shape estável —
    /// espelha <c>TrackSubmitResponse</c> em
    /// <c>PhishGuard.Frontend/src/shared/trackingContract.ts</c>.
    /// </summary>
    public class TrackSubmitResponseDto
    {
        /// <summary>Rótulo do evento registrado (ex.: "Inseriu Dados").</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// URL canônica da tela educacional para a landing redirecionar o alvo.
        /// Formato garantido pelo <c>TrackingContract</c> (parâmetros c/t).
        /// </summary>
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
