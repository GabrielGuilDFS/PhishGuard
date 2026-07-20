namespace PhishGuard.Backend.DTOs
{
    /// <summary>
    /// Ponto do gráfico de barras por campanha do dashboard.
    ///
    /// <see cref="Label"/> é o rótulo concatenado "Nome da Campanha | Mês" (mês de
    /// disparo, extraído de <c>Campaign.DataInicio</c>); <see cref="Clicks"/> é o total
    /// de cliques ÚNICOS da campanha, aplicando a regra de par único
    /// (<c>CampaignId</c> + <c>TargetId</c>): 1 clique por usuário por campanha.
    /// </summary>
    public class CampaignClicksDto
    {
        public string Label { get; set; } = string.Empty;
        public int Clicks { get; set; }
    }
}
