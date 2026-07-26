using System;

namespace PhishGuard.Backend.Utilities
{
    /// <summary>
    /// Conversão de data/hora para o fuso oficial de Brasília (America/Sao_Paulo, UTC-3),
    /// usada para carimbar horários legíveis nos e-mails de simulação (expiração de link,
    /// "acesso detectado", etc.).
    ///
    /// MOTIVO: o backend roda em contêiner com relógio em UTC. Usar <c>DateTime.Now</c> nesse
    /// ambiente devolve o horário UTC, que ao ser rotulado como "(BRT)" saía 3 horas ADIANTADO.
    /// Aqui partimos SEMPRE de <see cref="DateTime.UtcNow"/> e convertemos explicitamente para
    /// o fuso de Brasília, de modo que o horário impresso condiz com o momento real do envio,
    /// independentemente do fuso do host.
    /// </summary>
    public static class HorarioBrasilia
    {
        // IANA ("America/Sao_Paulo") funciona no Linux e, via ICU, no Windows moderno; o id
        // do Windows ("E. South America Standard Time") é o fallback para runtimes sem ICU.
        private static readonly TimeZoneInfo Fuso = ResolverFuso();

        private static TimeZoneInfo ResolverFuso()
        {
            foreach (var id in new[] { "America/Sao_Paulo", "E. South America Standard Time" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch (TimeZoneNotFoundException) { /* tenta o próximo id */ }
                catch (InvalidTimeZoneException) { /* tenta o próximo id */ }
            }

            // Último recurso: offset fixo UTC-3 (Brasília não observa horário de verão desde 2019).
            return TimeZoneInfo.CreateCustomTimeZone("BRT-3", TimeSpan.FromHours(-3), "Horário de Brasília", "BRT");
        }

        /// <summary>Horário de parede de Brasília no instante atual.</summary>
        public static DateTime Agora() => Converter(DateTime.UtcNow);

        /// <summary>
        /// Converte um instante UTC para o horário de parede de Brasília. O <paramref name="utc"/>
        /// é tratado como UTC (o <c>Kind</c> é normalizado) para evitar
        /// <see cref="ArgumentException"/> em <see cref="TimeZoneInfo.ConvertTimeFromUtc"/>.
        /// </summary>
        public static DateTime Converter(DateTime utc)
            => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Fuso);
    }
}
