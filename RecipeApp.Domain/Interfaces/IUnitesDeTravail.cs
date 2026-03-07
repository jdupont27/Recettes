using RecipeApp.Domain.Entities;

namespace RecipeApp.Domain.Interfaces;

public interface IUnitesDeTravail : IAsyncDisposable
{
    IRecetteRepository Recettes { get; }
    Task<int> SauvegarderAsync(CancellationToken annulation = default);

    /// <summary>
    /// Remplace les ingrédients et étapes d'une recette dans une seule transaction,
    /// en utilisant des DELETE SQL directs pour éviter les problèmes de tracking EF Core.
    /// </summary>
    Task RemplacerCollectionsRecetteAsync(
        Guid recetteId,
        IList<Ingredient> ingredients,
        IList<EtapeRecette> etapes,
        CancellationToken annulation = default);
}
