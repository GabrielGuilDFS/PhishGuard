namespace PhishGuard.Backend.Models
{
    /// <summary>
    /// Planos comerciais contratáveis por um Tenant. O valor numérico é o que
    /// fica persistido na coluna (enum -> int), então a ordem não deve mudar.
    /// </summary>
    public enum PlanoTenant
    {
        Bronze = 0,
        Prata = 1,
        Ouro = 2
    }

    /// <summary>
    /// Fonte única de verdade das cotas por plano. Mantida junto ao enum para
    /// que Frontend (via endpoint de quota) e Backend compartilhem as mesmas
    /// regras de negócio do MVP.
    /// </summary>
    public static class PlanoLimites
    {
        public const int LimiteAlvosBronze = 50;
        public const int LimiteAlvosPrata = 500;

        /// <summary>
        /// Quantidade máxima de alvos (colaboradores) que o plano permite cadastrar.
        /// O plano Ouro (Enterprise) é ilimitado no escopo atual.
        /// </summary>
        public static int LimiteDeAlvos(PlanoTenant plano) => plano switch
        {
            PlanoTenant.Bronze => LimiteAlvosBronze,
            PlanoTenant.Prata => LimiteAlvosPrata,
            PlanoTenant.Ouro => int.MaxValue,
            _ => LimiteAlvosBronze
        };

        /// <summary>
        /// Converte o identificador textual usado no fluxo de onboarding
        /// ("bronze" | "prata" | "ouro") para o enum, com fallback em Bronze.
        /// </summary>
        public static PlanoTenant DeTexto(string? plano) => (plano ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "prata" => PlanoTenant.Prata,
            "ouro" => PlanoTenant.Ouro,
            _ => PlanoTenant.Bronze
        };
    }
}
