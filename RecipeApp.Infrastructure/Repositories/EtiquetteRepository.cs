using Microsoft.EntityFrameworkCore;
using RecipeApp.Domain.Entities;
using RecipeApp.Domain.Interfaces;
using RecipeApp.Infrastructure.Persistence;

namespace RecipeApp.Infrastructure.Repositories;

public class EtiquetteRepository : IEtiquetteRepository
{
    private readonly AppDbContext _contexte;

    public EtiquetteRepository(AppDbContext contexte)
    {
        _contexte = contexte;
    }

    public async Task<IEnumerable<Etiquette>> ObtenirToutesAsync(CancellationToken annulation = default)
    {
        return await _contexte.Etiquettes.OrderBy(e => e.Nom).ToListAsync(annulation);
    }

    public async Task<Etiquette?> ObtenirParIdAsync(Guid id, CancellationToken annulation = default)
    {
        return await _contexte.Etiquettes.FindAsync([id], annulation);
    }

    public async Task<Etiquette> AjouterAsync(Etiquette etiquette, CancellationToken annulation = default)
    {
        await _contexte.Etiquettes.AddAsync(etiquette, annulation);
        return etiquette;
    }
}
