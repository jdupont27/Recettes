using RecipeApp.Domain.Entities;

namespace RecipeApp.Domain.Interfaces;

public interface IRecetteRepository
{
    Task<Recette?> ObtenirParIdAsync(Guid id, CancellationToken annulation = default);
    Task<(IEnumerable<Recette> Recettes, int Total)> RechercherAsync(FiltreRecette filtre, CancellationToken annulation = default);
    Task<Recette> AjouterAsync(Recette recette, CancellationToken annulation = default);
    Task<Recette> ModifierAsync(Recette recette, CancellationToken annulation = default);
    Task SupprimerAsync(Guid id, CancellationToken annulation = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken annulation = default);
    Task<IEnumerable<Recette>> ObtenirParAuteurAsync(Guid auteurId, CancellationToken annulation = default);
    Task<IEnumerable<Recette>> ObtenirPartagesesAvecAsync(Guid utilisateurId, CancellationToken annulation = default);
    Task PartagerAvecAsync(Guid recetteId, Guid utilisateurId, CancellationToken annulation = default);
    Task RetirerPartageAsync(Guid recetteId, Guid utilisateurId, CancellationToken annulation = default);
}