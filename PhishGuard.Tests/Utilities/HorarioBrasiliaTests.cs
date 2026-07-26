using System;
using PhishGuard.Backend.Utilities;
using Xunit;

namespace PhishGuard.Tests.Utilities;

// Cobre o carimbo de data/hora dos e-mails de simulação (ex.: card de dispositivo da
// isca "Mercado Liv", placeholder {{DATA_ACESSO}}). O horário DEVE refletir o fuso
// oficial de Brasília (America/Sao_Paulo), independentemente do fuso do host — no
// contêiner o relógio é UTC, então a conversão explícita evita o horário sair 3h
// adiantado. O Brasil não observa horário de verão desde 2019, logo UTC-3 o ano todo.
public class HorarioBrasiliaTests
{
    [Theory]
    [InlineData(2026, 1, 15, 12, 0, 9, 0)]   // verão do hemisfério sul, mas sem DST → UTC-3
    [InlineData(2026, 7, 26, 15, 30, 12, 30)] // inverno → UTC-3
    public void Converter_DeUtc_ParaFusoDeBrasilia_AplicaUtcMenos3(
        int ano, int mes, int dia, int horaUtc, int minUtc, int horaBrt, int minBrt)
    {
        var utc = new DateTime(ano, mes, dia, horaUtc, minUtc, 0, DateTimeKind.Utc);

        var brt = HorarioBrasilia.Converter(utc);

        Assert.Equal(new DateTime(ano, mes, dia, horaBrt, minBrt, 0), brt);
    }

    [Fact]
    public void Converter_NormalizaKindNaoUtc_SemLancar()
    {
        // DataHora com Kind=Unspecified não pode derrubar a conversão (SpecifyKind interno).
        var unspecified = new DateTime(2026, 7, 26, 18, 0, 0, DateTimeKind.Unspecified);

        var ex = Record.Exception(() => HorarioBrasilia.Converter(unspecified));

        Assert.Null(ex);
    }

    [Fact]
    public void Agora_UsaUtcNow_NaoOHorarioLocalDoHost()
    {
        // Agora() = Converter(UtcNow); deve casar (na janela de segundos) com a conversão
        // manual do UtcNow — provando que parte de UTC e não do relógio local do host.
        var esperado = HorarioBrasilia.Converter(DateTime.UtcNow);

        var agora = HorarioBrasilia.Agora();

        Assert.True(Math.Abs((agora - esperado).TotalSeconds) < 5,
            "Agora() deveria derivar de DateTime.UtcNow convertido para Brasília.");
    }
}
