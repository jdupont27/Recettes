using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _contexte;

    public UnitOfWork(AppDbContext contexte)
    {
        _contexte = contexte;
    }

    public async Task<int> SauvegarderAsync(CancellationToken annulation = default)
    {
        return await _contexte.SaveChangesAsync(annulation);
    }
}
