using RecipeApp.Domain.Entities;

namespace RecipeApp.Domain.Interfaces;

public interface IEtiquetteRepository
{
    Task<IEnumerable<Etiquette>> ObtenirToutesAsync(CancellationToken annulation = default);
    Task<Etiquette?> ObtenirParIdAsync(Guid id, CancellationToken annulation = default);
    Task<Etiquette> AjouterAsync(Etiquette etiquette, CancellationToken annulation = default);
}