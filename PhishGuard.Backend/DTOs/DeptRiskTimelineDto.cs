namespace PhishGuard.Backend.DTOs
{
    /// <summary>
    /// Resposta do endpoint <c>GET /api/Dashboard/dept-risk-timeline</c>.
    ///
    /// <see cref="Departamentos"/> lista os nomes de todos os departamentos presentes na
    /// série para que o frontend saiba quantas linhas desenhar no gráfico.
    ///
    /// <see cref="Timeline"/> é a série cronológica: cada elemento representa um mês e
    /// contém, além de <c>mes</c> (rótulo "Jan/25") e <c>ordem</c> (yyyyMM para
    /// ordenação), chaves dinâmicas <c>{Dept}_cliques</c> e <c>{Dept}_submissoes</c>
    /// com as taxas percentuais (0–100) de cada departamento naquele mês.
    /// </summary>
    public class DeptRiskTimelineResponse
    {
        public List<string> Departamentos { get; set; } = new();
        public List<Dictionary<string, object>> Timeline { get; set; } = new();
    }
}
