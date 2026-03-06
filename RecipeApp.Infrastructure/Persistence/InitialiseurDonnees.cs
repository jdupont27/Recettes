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
        if (await contexte.Categories.AnyAsync())
            return;

        var categories = new List<Categorie>
        {
            new() { Id = Guid.NewGuid(), Nom = "Entrée" },
            new() { Id = Guid.NewGuid(), Nom = "Plat principal" },
            new() { Id = Guid.NewGuid(), Nom = "Dessert" },
            new() { Id = Guid.NewGuid(), Nom = "Soupe" },
            new() { Id = Guid.NewGuid(), Nom = "Salade" },
            new() { Id = Guid.NewGuid(), Nom = "Apéritif" },
            new() { Id = Guid.NewGuid(), Nom = "Boisson" },
            new() { Id = Guid.NewGuid(), Nom = "Pain & Viennoiserie" }
        };

        await contexte.Categories.AddRangeAsync(categories);
        await contexte.SaveChangesAsync();
    }

    private static async Task InitialiserEtiquettesAsync(AppDbContext contexte)
    {
        if (await contexte.Etiquettes.AnyAsync())
            return;

        var etiquettes = new List<Etiquette>
        {
            new() { Id = Guid.NewGuid(), Nom = "Végétarien" },
            new() { Id = Guid.NewGuid(), Nom = "Vegan" },
            new() { Id = Guid.NewGuid(), Nom = "Sans gluten" },
            new() { Id = Guid.NewGuid(), Nom = "Sans lactose" },
            new() { Id = Guid.NewGuid(), Nom = "Épicé" },
            new() { Id = Guid.NewGuid(), Nom = "Rapide" },
            new() { Id = Guid.NewGuid(), Nom = "Économique" },
            new() { Id = Guid.NewGuid(), Nom = "Sans noix" }
        };

        await contexte.Etiquettes.AddRangeAsync(etiquettes);
        await contexte.SaveChangesAsync();
    }
}
