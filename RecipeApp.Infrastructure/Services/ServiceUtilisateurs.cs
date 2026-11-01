using Microsoft.EntityFrameworkCore;
using RecipeApp.Application.Interfaces;
using RecipeApp.Infrastructure.Persistence;

namespace RecipeApp.Infrastructure.Services;

public class ServiceUtilisateurs : IServiceUtilisateurs
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ServiceUtilisateurs(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<string?> ObtenirNomAsync(Guid utilisateurId, CancellationToken annulation = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(annulation);
        return await ctx.Users
            .Where(u => u.Id == utilisateurId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync(annulation);
    }
}
