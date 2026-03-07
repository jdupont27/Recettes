using Microsoft.EntityFrameworkCore;
using RecipeApp.Domain.Entities;
using RecipeApp.Domain.Interfaces;
using RecipeApp.Infrastructure.Persistence;

namespace RecipeApp.Infrastructure.Repositories;

public class EtiquetteRepository : IEtiquetteRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public EtiquetteRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Etiquette>> ObtenirToutesAsync(CancellationToken annulation = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(annulation);
        return await ctx.Etiquettes.OrderBy(e => e.Nom).ToListAsync(annulation);
    }

    public async Task<Etiquette?> ObtenirParIdAsync(Guid id, CancellationToken annulation = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(annulation);
        return await ctx.Etiquettes.FindAsync([id], annulation);
    }

    public async Task<Etiquette> AjouterAsync(Etiquette etiquette, CancellationToken annulation = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(annulation);
        await ctx.Etiquettes.AddAsync(etiquette, annulation);
        await ctx.SaveChangesAsync(annulation);
        return etiquette;
    }
}
