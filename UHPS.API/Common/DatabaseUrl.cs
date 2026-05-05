namespace UHPS.API.Common;

// Railway, Heroku, Render, and most managed-Postgres providers expose the database
// via a DATABASE_URL env var in URI form: postgres://user:pass@host:port/dbname.
// Npgsql wants key-value form. This converts when DATABASE_URL is set, returning
// null otherwise so the caller can fall back to the appsettings connection string.
public static class DatabaseUrl
{
    public static string? FromEnvVar()
    {
        var url = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(url)) return null;

        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');

        // SSL Mode=Require + Trust Server Certificate=true: managed Postgres providers
        // require TLS and use self-signed certs; trusting the cert is the practical default.
        return $"Host={uri.Host};Port={uri.Port};Database={database};" +
               $"Username={user};Password={pass};" +
               "SSL Mode=Require;Trust Server Certificate=true";
    }
}
