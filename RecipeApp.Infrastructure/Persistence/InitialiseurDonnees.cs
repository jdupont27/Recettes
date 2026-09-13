using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeApp.Domain.Entities;

namespace RecipeApp.Infrastructure.Persistence;

/// <summary>Initialise la base de données avec les données de référence par défaut.</summary>
public static class InitialiseurDonnees
{
    public static async Task InitialiserAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var contexte = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await InitialiserCategoriesAsync(contexte);
        await InitialiserEtiquettesAsync(contexte);
    }

    private static async Task InitialiserCategoriesAsync(AppDbContext contexte)
    {
        var nomsParDefaut = new[]
        {
            "Entrée",
            "Plat principal",
            "Dessert",
            "Soupe",
            "Salade",
            "Apéritif",
            "Boisson",
            "Pain & Viennoiserie",
            "Accompagnement",
            "Vinaigrettes et Marinades",
            "Légumes"
        };

        var nomsExistants = await contexte.Categories.Select(c => c.Nom).ToListAsync();
        var categoriesManquantes = nomsParDefaut
            .Where(nom => !nomsExistants.Contains(nom))
            .Select(nom => new Categorie { Id = Guid.NewGuid(), Nom = nom })
            .ToList();

        if (categoriesManquantes.Count == 0)
            return;

        await contexte.Categories.AddRangeAsync(categoriesManquantes);
        await contexte.SaveChangesAsync();
    }

    private static async Task InitialiserEtiquettesAsync(AppDbContext contexte)
    {
        var nomsParDefaut = new[]
        {
            "Végétarien",
            "Vegan",
            "Sans gluten",
            "Sans lactose",
            "Épicé",
            "Rapide",
            "Économique",
            "Sans noix"
        };

        var nomsExistants = await contexte.Etiquettes.Select(e => e.Nom).ToListAsync();
        var etiquettesManquantes = nomsParDefaut
            .Where(nom => !nomsExistants.Contains(nom))
            .Select(nom => new Etiquette { Id = Guid.NewGuid(), Nom = nom })
            .ToList();

        if (etiquettesManquantes.Count == 0)
            return;

        await contexte.Etiquettes.AddRangeAsync(etiquettesManquantes);
        await contexte.SaveChangesAsync();
    }
}
