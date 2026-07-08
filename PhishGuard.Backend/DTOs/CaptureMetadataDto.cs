namespace PhishGuard.Backend.DTOs
{
    /// <summary>
    /// Metadados de validação enviados pela landing page simulada no momento
    /// da submissão do formulário.
    ///
    /// IMPORTANTE (LGPD / escopo educacional): este contrato carrega APENAS
    /// propriedades de validação (flags e tamanhos). A senha real digitada pelo
    /// alvo NUNCA é transmitida nem persistida. O que interessa para a métrica do
    /// TCC é o EVENTO "Inseriu Dados", não o conteúdo sensível.
    /// </summary>
    public class CaptureMetadataDto
    {
        /// <summary>Indica se todos os campos obrigatórios foram preenchidos.</summary>
        public bool CamposPreenchidos { get; set; }

        /// <summary>Indica se a senha e a confirmação coincidiam.</summary>
        public bool SenhasCoincidem { get; set; }

        /// <summary>Comprimento da senha digitada (apenas o número de caracteres, nunca o texto).</summary>
        public int TamanhoSenha { get; set; }
    }
}
