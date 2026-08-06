using Npgsql;
using PhishGuard.Backend.Data;

namespace PhishGuard.Tests.Data;

public sealed class DatabaseConnectionStringTests
{
    [Fact]
    public void Normalize_PreservaConnectionStringNativa()
    {
        const string original = "Host=localhost;Port=5433;Database=phishguard;Username=postgres;Password=secret";

        Assert.Equal(original, DatabaseConnectionString.Normalize(original));
    }

    [Fact]
    public void Normalize_ConverteUriRenderEDecodificaCredenciais()
    {
        var normalized = DatabaseConnectionString.Normalize(
            "postgresql://user%40tenant:p%40ss%3Aword@render.example.com:6543/phish%20guard?sslmode=require");
        var parsed = new NpgsqlConnectionStringBuilder(normalized);

        Assert.Equal("render.example.com", parsed.Host);
        Assert.Equal(6543, parsed.Port);
        Assert.Equal("phish guard", parsed.Database);
        Assert.Equal("user@tenant", parsed.Username);
        Assert.Equal("p@ss:word", parsed.Password);
        Assert.Equal(SslMode.Require, parsed.SslMode);
    }

    [Theory]
    [InlineData("verify-ca", SslMode.VerifyCA)]
    [InlineData("verify-full", SslMode.VerifyFull)]
    [InlineData("disable", SslMode.Disable)]
    public void Normalize_TrataSslModeDaQuery(string sslMode, SslMode expected)
    {
        var normalized = DatabaseConnectionString.Normalize(
            $"postgres://user:password@host/database?sslmode={sslMode}");

        Assert.Equal(expected, new NpgsqlConnectionStringBuilder(normalized).SslMode);
    }

    [Fact]
    public void Normalize_RejeitaSslModeDesconhecido()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            DatabaseConnectionString.Normalize(
                "postgres://user:password@host/database?sslmode=unsafe"));

        Assert.Contains("SSL Mode inválido", exception.Message);
    }
}
