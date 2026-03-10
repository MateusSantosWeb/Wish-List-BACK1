using Npgsql;

namespace WishListAPI.Data;

public static class DbConnectionString
{
    public static string? NormalizePostgres(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw;

        // Npgsql normalmente usa "Host=...;Database=...;Username=...;Password=...".
        // Alguns provedores entregam no formato URI:
        // postgresql://user:pass@host:port/db?sslmode=require
        if (!raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return raw;
        }

        var uri = new Uri(raw);

        var user = "";
        var pass = "";
        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var parts = uri.UserInfo.Split(':', 2, StringSplitOptions.None);
            user = Uri.UnescapeDataString(parts[0]);
            if (parts.Length > 1)
                pass = Uri.UnescapeDataString(parts[1]);
        }

        var db = uri.AbsolutePath.Trim('/');

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Username = user,
            Password = pass,
            Database = db,
            Pooling = true,
        };

        // Parse querystring sem dependencias extras.
        var q = uri.Query.TrimStart('?');
        if (!string.IsNullOrWhiteSpace(q))
        {
            foreach (var pair in q.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var kv = pair.Split('=', 2, StringSplitOptions.TrimEntries);
                var key = Uri.UnescapeDataString(kv[0]);
                var val = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : "";

                if (key.Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                {
                    if (val.Equals("require", StringComparison.OrdinalIgnoreCase))
                        builder.SslMode = SslMode.Require;
                    else if (val.Equals("verify-full", StringComparison.OrdinalIgnoreCase) || val.Equals("verifyfull", StringComparison.OrdinalIgnoreCase))
                        builder.SslMode = SslMode.VerifyFull;
                    else if (val.Equals("disable", StringComparison.OrdinalIgnoreCase))
                        builder.SslMode = SslMode.Disable;
                }
                // Ignora params desconhecidos (ex: channel_binding) pra evitar erro de parsing no Npgsql.
            }
        }

        // Se nao foi especificado, assume SSL Require (mais comum em Postgres online).
        if (builder.SslMode == SslMode.Prefer)
            builder.SslMode = SslMode.Require;

        return builder.ConnectionString;
    }
}

