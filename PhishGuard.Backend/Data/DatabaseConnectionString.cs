using Microsoft.AspNetCore.WebUtilities;
using Npgsql;

namespace PhishGuard.Backend.Data;

public static class DatabaseConnectionString
{
    public static string Normalize(string? configuredValue)
    {
        if (string.IsNullOrWhiteSpace(configuredValue))
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' não foi configurada.");

        var value = configuredValue.Trim();
        if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new InvalidOperationException(
                "A URI PostgreSQL de 'DefaultConnection' é inválida.");
        }

        var separator = uri.UserInfo.IndexOf(':');
        if (separator <= 0)
            throw new InvalidOperationException(
                "A URI PostgreSQL deve informar usuário e senha.");

        var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        if (string.IsNullOrWhiteSpace(database))
            throw new InvalidOperationException(
                "A URI PostgreSQL deve informar o nome do banco de dados.");

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = database,
            Username = Uri.UnescapeDataString(uri.UserInfo[..separator]),
            Password = Uri.UnescapeDataString(uri.UserInfo[(separator + 1)..])
        };

        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("sslmode", out var sslModeValue))
            builder.SslMode = ParseSslMode(sslModeValue.ToString());
        else if (query.TryGetValue("ssl", out var sslValue)
                 && bool.TryParse(sslValue.ToString(), out var sslEnabled))
            builder.SslMode = sslEnabled ? SslMode.Require : SslMode.Disable;

        return builder.ConnectionString;
    }

    private static SslMode ParseSslMode(string value) =>
        value.Trim().ToLowerInvariant().Replace("_", "-") switch
        {
            "disable" => SslMode.Disable,
            "allow" => SslMode.Allow,
            "prefer" => SslMode.Prefer,
            "require" => SslMode.Require,
            "verify-ca" => SslMode.VerifyCA,
            "verify-full" => SslMode.VerifyFull,
            _ => throw new InvalidOperationException(
                $"SSL Mode inválido na URI PostgreSQL: '{value}'.")
        };
}
