using Microsoft.EntityFrameworkCore;
using RecipeApp.Domain.Entities;
using RecipeApp.Domain.Interfaces;
using RecipeApp.Infrastructure.Persistence;

namespace RecipeApp.Infrastructure.Repositories;

public class CategorieRepository : ICategorieRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CategorieRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Categorie>> ObtenirToutesAsync(CancellationToken annulation = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(annulation);
        return await ctx.Categories.OrderBy(c => c.Nom).ToListAsync(annulation);
    }

    public async Task<Categorie?> ObtenirParIdAsync(Guid id, CancellationToken annulation = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(annulation);
        return await ctx.Categories.FindAsync([id], annulation);
    }

    public async Task<Categorie> AjouterAsync(Categorie categorie, CancellationToken annulation = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(annulation);
        await ctx.Categories.AddAsync(categorie, annulation);
        await ctx.SaveChangesAsync(annulation);
        return categorie;
    }
}
