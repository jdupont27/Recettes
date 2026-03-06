using Microsoft.EntityFrameworkCore;
using RecipeApp.Domain.Entities;
using RecipeApp.Domain.Interfaces;
using RecipeApp.Infrastructure.Persistence;

namespace RecipeApp.Infrastructure.Repositories;

public class CategorieRepository : ICategorieRepository
{
    private readonly AppDbContext _contexte;

    public CategorieRepository(AppDbContext contexte)
    {
        _contexte = contexte;
    }

    public async Task<IEnumerable<Categorie>> ObtenirToutesAsync(CancellationToken annulation = default)
    {
        return await _contexte.Categories.OrderBy(c => c.Nom).ToListAsync(annulation);
    }

    public async Task<Categorie?> ObtenirParIdAsync(Guid id, CancellationToken annulation = default)
    {
        return await _contexte.Categories.FindAsync([id], annulation);
    }

    public async Task<Categorie> AjouterAsync(Categorie categorie, CancellationToken annulation = default)
    {
        await _contexte.Categories.AddAsync(categorie, annulation);
        return categorie;
    }
}
