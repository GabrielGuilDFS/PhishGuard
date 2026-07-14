namespace PhishGuard.Backend.Models
{
    /// <summary>
    /// Constantes de status de uma campanha. Fluxo canônico:
    /// Rascunho → Agendada → Processando → Em Andamento → Finalizada.
    ///
    /// - Rascunho:    recém-criada; ainda não ativada pelo administrador.
    /// - Agendada:    ativada com DataInicio no futuro; aguarda o worker no horário.
    /// - Processando: CLAIM do disparo — o worker (ou o /ativar imediato) reivindicou a
    ///                campanha e o lote de e-mails está sendo enviado de forma assíncrona.
    ///                É o estado que garante a RETOMADA idempotente: se o processo cair no
    ///                meio, a campanha continua "Processando" e o worker a reprocessa,
    ///                pulando os alvos que já receberam (log de Envio).
    /// - Em Andamento: lote 100% disparado; coletando interações (abertura/clique/submissão).
    /// - Finalizada:  coleta encerrada (limite da Data de Encerramento da Coleta).
    /// </summary>
    public static class CampaignStatus
    {
        public const string Rascunho = "Rascunho";
        public const string Agendada = "Agendada";
        public const string Processando = "Processando";
        public const string EmAndamento = "Em Andamento";
        public const string Finalizada = "Finalizada";
    }
}
