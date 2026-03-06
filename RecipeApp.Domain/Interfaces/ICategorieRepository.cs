using RecipeApp.Domain.Entities;

namespace RecipeApp.Domain.Interfaces;

public interface ICategorieRepository
{
    Task<IEnumerable<Categorie>> ObtenirToutesAsync(CancellationToken annulation = default);
    Task<Categorie?> ObtenirParIdAsync(Guid id, CancellationToken annulation = default);
    Task<Categorie> AjouterAsync(Categorie categorie, CancellationToken annulation = default);
}