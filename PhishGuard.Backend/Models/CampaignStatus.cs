namespace PhishGuard.Backend.Models
{
    /// <summary>
    /// Constantes de status de uma campanha. Fluxo canônico:
    /// Rascunho → Agendada → Em Andamento → Finalizada.
    ///
    /// - Rascunho:    recém-criada; ainda não ativada pelo administrador.
    /// - Agendada:    ativada com DataInicio no futuro; aguarda o worker no horário.
    /// - Em Andamento: e-mails disparados (imediato ou pelo worker de agendamento).
    /// - Finalizada:  coleta encerrada (limite da Data de Encerramento da Coleta).
    /// </summary>
    public static class CampaignStatus
    {
        public const string Rascunho = "Rascunho";
        public const string Agendada = "Agendada";
        public const string EmAndamento = "Em Andamento";
        public const string Finalizada = "Finalizada";
    }
}
