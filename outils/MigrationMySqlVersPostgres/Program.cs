// Outil ponctuel : copie toutes les données de l'ancienne base MySQL (Aiven)
// vers la nouvelle base PostgreSQL (Aiven).
//
// Utilisation (depuis la racine du repo) :
//   dotnet run --project outils/MigrationMySqlVersPostgres -- "<chaîne MySQL>" "<chaîne ou URI PostgreSQL>"
//
// La base PostgreSQL cible doit être vide (aucun utilisateur ni recette).
// Le schéma est créé automatiquement s'il n'existe pas encore.

using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecipeApp.Domain.Entities;
using RecipeApp.Infrastructure.Identity;
using RecipeApp.Infrastructure.Persistence;

if (args.Length < 2)
{
    Console.Error.WriteLine("""
        Utilisation :
          dotnet run --project outils/MigrationMySqlVersPostgres -- "<chaîne MySQL source>" "<chaîne ou URI PostgreSQL cible>"

        Exemple :
          dotnet run --project outils/MigrationMySqlVersPostgres -- ^
            "Server=xxx.aivencloud.com;Port=12345;Database=defaultdb;Uid=avnadmin;Pwd=...;SslMode=Required;" ^
            "postgres://avnadmin:...@yyy.aivencloud.com:12345/defaultdb?sslmode=require"
        """);
    return 1;
}

var chaineMySql = args[0];
var chainePostgres = ChaineConnexionPostgres.Normaliser(args[1]);

var optionsSource = new DbContextOptionsBuilder<AppDbContext>()
    .UseMySql(chaineMySql, ServerVersion.AutoDetect(chaineMySql))
    .Options;

var optionsCible = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(chainePostgres)
    .Options;

await using var source = new AppDbContext(optionsSource);
await using var cible = new AppDbContext(optionsCible);

Console.WriteLine("Création du schéma PostgreSQL (si absent)...");
await cible.Database.EnsureCreatedAsync();

if (await cible.Users.AnyAsync() || await cible.Recettes.AnyAsync())
{
    Console.Error.WriteLine("La base PostgreSQL contient déjà des utilisateurs ou des recettes. Migration annulée.");
    return 1;
}

await using var transaction = await cible.Database.BeginTransactionAsync();

// Si l'application a déjà démarré sur la nouvelle base, les catégories/étiquettes
// par défaut y ont été créées avec d'autres identifiants : on les remplace par celles de MySQL.
await cible.RecetteCategories.ExecuteDeleteAsync();
await cible.RecetteEtiquettes.ExecuteDeleteAsync();
await cible.Categories.ExecuteDeleteAsync();
await cible.Etiquettes.ExecuteDeleteAsync();

// Ordre respectant les clés étrangères
await CopierAsync<IdentityRole<Guid>>("Roles");
await CopierAsync<ApplicationUser>("Utilisateurs");
await CopierAsync<IdentityRoleClaim<Guid>>("RolesClaims");
await CopierAsync<IdentityUserClaim<Guid>>("UtilisateursClaims");
await CopierAsync<IdentityUserLogin<Guid>>("UtilisateursConnexions");
await CopierAsync<IdentityUserRole<Guid>>("UtilisateursRoles");
await CopierAsync<IdentityUserToken<Guid>>("UtilisateursTokens");
await CopierAsync<Categorie>("Categories");
await CopierAsync<Etiquette>("Etiquettes");
await CopierAsync<Recette>("Recettes");
await CopierAsync<Ingredient>("Ingredients");
await CopierAsync<EtapeRecette>("EtapesRecette");
await CopierAsync<PartageRecette>("PartagesRecette");
await CopierAsync<RecetteCategorie>("RecetteCategories");
await CopierAsync<RecetteEtiquette>("RecetteEtiquettes");

// Les tables de claims ont une clé entière auto-incrémentée : on réaligne leur séquence
// pour que les prochains INSERT ne tombent pas sur un identifiant déjà utilisé.
foreach (var table in new[] { "RolesClaims", "UtilisateursClaims" })
{
#pragma warning disable EF1002 // noms de tables fixes, pas d'entrée utilisateur
    await cible.Database.ExecuteSqlRawAsync(
        $"""SELECT setval(pg_get_serial_sequence('"{table}"', 'Id'), COALESCE((SELECT MAX("Id") FROM "{table}"), 0) + 1, false);""");
#pragma warning restore EF1002
}

await transaction.CommitAsync();
Console.WriteLine("Migration terminée avec succès.");
return 0;

async Task CopierAsync<T>(string nom) where T : class
{
    var elements = await source.Set<T>().AsNoTracking().ToListAsync();
    foreach (var element in elements)
        ConvertirDatesEnUtc(element);

    cible.Set<T>().AddRange(elements);
    await cible.SaveChangesAsync();
    cible.ChangeTracker.Clear();

    Console.WriteLine($"  {nom} : {elements.Count} ligne(s) copiée(s)");
}

// MySQL renvoie des DateTime sans fuseau (Kind=Unspecified) alors que PostgreSQL (timestamptz)
// exige de l'UTC. L'application enregistre toujours DateTime.UtcNow, donc on marque simplement les valeurs en UTC.
static void ConvertirDatesEnUtc(object element)
{
    foreach (var propriete in element.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
    {
        if (!propriete.CanRead || !propriete.CanWrite)
            continue;

        var valeur = propriete.GetValue(element);
        switch (valeur)
        {
            case DateTime date:
                propriete.SetValue(element, DateTime.SpecifyKind(date, DateTimeKind.Utc));
                break;
            case DateTimeOffset dateOffset:
                propriete.SetValue(element, dateOffset.ToUniversalTime());
                break;
        }
    }
}
