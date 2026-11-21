using Npgsql;

namespace RecipeApp.Infrastructure.Persistence;

/// <summary>
/// Accepte une chaîne de connexion PostgreSQL sous forme clé=valeur (format Npgsql)
/// ou sous forme d'URI (format fourni par Aiven : postgres://user:mdp@hote:port/bd?sslmode=require).
/// </summary>
public static class ChaineConnexionPostgres
{
    public static string Normaliser(string chaine)
    {
        chaine = chaine.Trim();

        if (!chaine.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !chaine.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return chaine;
        }

        var uri = new Uri(chaine);
        var infosUtilisateur = uri.UserInfo.Split(':', 2);

        var constructeur = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(infosUtilisateur[0]),
            SslMode = SslMode.Require
        };

        if (infosUtilisateur.Length > 1)
            constructeur.Password = Uri.UnescapeDataString(infosUtilisateur[1]);

        foreach (var parametre in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var morceaux = parametre.Split('=', 2);
            if (morceaux.Length != 2 || !morceaux[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                continue;

            constructeur.SslMode = Uri.UnescapeDataString(morceaux[1]).ToLowerInvariant() switch
            {
                "disable" => SslMode.Disable,
                "allow" => SslMode.Allow,
                "prefer" => SslMode.Prefer,
                "verify-ca" => SslMode.VerifyCA,
                "verify-full" => SslMode.VerifyFull,
                _ => SslMode.Require
            };
        }

        return constructeur.ConnectionString;
    }
}
