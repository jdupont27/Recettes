using Microsoft.EntityFrameworkCore;
using RecipeApp.Domain.Entities;
using RecipeApp.Domain.Interfaces;
using RecipeApp.Infrastructure.Repositories;

namespace RecipeApp.Infrastructure.Persistence;

public class UnitesDeTravail : IUnitesDeTravail
{
    private readonly AppDbContext _contexte;

    public IRecetteRepository Recettes { get; }

    public UnitesDeTravail(IDbContextFactory<AppDbContext> factory)
    {
        _contexte = factory.CreateDbContext();
        Recettes = new RecetteRepository(_contexte);
    }

    public Task<int> SauvegarderAsync(CancellationToken annulation = default)
        => _contexte.SaveChangesAsync(annulation);

    public async Task RemplacerCollectionsRecetteAsync(
        Guid recetteId,
        IList<Ingredient> ingredients,
        IList<EtapeRecette> etapes,
        CancellationToken annulation = default)
    {
        // DELETE SQL direct — contourne le tracking EF qui cause DbUpdateConcurrencyException
        await _contexte.Ingredients
            .Where(i => i.RecetteId == recetteId)
            .ExecuteDeleteAsync(annulation);

        await _contexte.EtapesRecette
            .Where(e => e.RecetteId == recetteId)
            .ExecuteDeleteAsync(annulation);

        if (ingredients.Count > 0)
            await _contexte.Ingredients.AddRangeAsync(ingredients, annulation);

        if (etapes.Count > 0)
            await _contexte.EtapesRecette.AddRangeAsync(etapes, annulation);
    }

    public async ValueTask DisposeAsync()
        => await _contexte.DisposeAsync();
}
