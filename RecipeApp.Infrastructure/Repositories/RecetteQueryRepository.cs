using Microsoft.EntityFrameworkCore;
using RecipeApp.Domain.Entities;
using RecipeApp.Domain.Enums;
using RecipeApp.Domain.Interfaces;
using RecipeApp.Infrastructure.Persistence;

namespace RecipeApp.Infrastructure.Repositories;

/// <summary>
/// Repository lecture seule — chaque méthode crée et dispose son propre DbContext
/// pour éviter les conflits de concurrence dans les circuits Blazor Server.
/// </summary>
public class RecetteQueryRepository : IRecetteRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public RecetteQueryRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Recette?> ObtenirParIdAsync(Guid id, CancellationToken annulation = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(annulation);
        return await ctx.Recettes
            .Include(r => r.Ingredients.OrderBy(i => i.Ordre))
            .Include(r => r.Etapes.OrderBy(e => e.NumeroEtape))
            .Include(r => r.Partages)
            .Include(r => r.RecetteCategories).ThenInclude(rc => rc.Categorie)
            .Include(r => r.RecetteEtiquettes).ThenInclude(re => re.Etiquette)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, annulation);
    }

    public async Task<Recette?> ObtenirParLienSecretAsync(string lienSecret, CancellationToken annulation = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(annulation);
        return await ctx.Recettes
            .Include(r => r.Ingredients.OrderBy(i => i.Ordre))
            .Include(r => r.Etapes.OrderBy(e => e.NumeroEtape))
            .Include(r => r.RecetteCategories).ThenInclude(rc => rc.Categorie)
            .Include(r => r.RecetteEtiquettes).ThenInclude(re => re.Etiquette)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.LienSecret == lienSecret && r.Visibilite == Visibilite.Partagee, annulation);
    }

    public async Task<(IEnumerable<Recette> Recettes, int Total)> RechercherAsync(FiltreRecette filtre, CancellationToken annulation = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(annulation);

        var requete = ctx.Recettes
            .Include(r => r.Ingredients)
            .Include(r => r.RecetteCategories).ThenInclude(rc => rc.Categorie)
            .Include(r => r.RecetteEtiquettes).ThenInclude(re => re.Etiquette)
            .Include(r => r.Partages)
            .AsNoTracking()
            .AsQueryable();

        if (filtre.UtilisateurConnecteId.HasValue)
        {
            var userId = filtre.UtilisateurConnecteId.Value;
            requete = requete.Where(r =>
                (filtre.IncluirePubliques && r.Visibilite == Visibilite.Publique) ||
                r.AuteurId == userId);
        }
        else
        {
            requete = requete.Where(r => r.Visibilite == Visibilite.Publique);
        }

        if (filtre.AuteurId.HasValue)
            requete = requete.Where(r => r.AuteurId == filtre.AuteurId.Value);

        if (!string.IsNullOrWhiteSpace(filtre.TexteLibre))
        {
            var texte = filtre.TexteLibre.ToLower();
            requete = requete.Where(r =>
                r.Titre.ToLower().Contains(texte) ||
                (r.Description != null && r.Description.ToLower().Contains(texte)) ||
                r.Ingredients.Any(i => i.Nom.ToLower().Contains(texte)));
        }

        if (filtre.TempsMaxMinutes.HasValue)
            requete = requete.Where(r => r.TempsPreparation + r.TempsCuisson <= filtre.TempsMaxMinutes.Value);

        if (filtre.NombrePortions.HasValue)
            requete = requete.Where(r => r.NombrePortions == filtre.NombrePortions.Value);

        if (filtre.CategorieId.HasValue)
            requete = requete.Where(r => r.RecetteCategories.Any(rc => rc.CategorieId == filtre.CategorieId.Value));

        if (!string.IsNullOrWhiteSpace(filtre.TypeCuisine))
            requete = requete.Where(r => r.TypeCuisine != null && r.TypeCuisine.ToLower().Contains(filtre.TypeCuisine.ToLower()));

        if (filtre.Difficulte.HasValue)
            requete = requete.Where(r => r.Difficulte == filtre.Difficulte.Value);

        foreach (var etiquetteId in filtre.EtiquettesIds)
        {
            var id = etiquetteId;
            requete = requete.Where(r => r.RecetteEtiquettes.Any(re => re.EtiquetteId == id));
        }

        if (filtre.IngredientsDisponibles.Count > 0)
        {
            var ingredients = filtre.IngredientsDisponibles.Select(i => i.ToLower()).ToList();
            requete = requete.Where(r => r.Ingredients.Any(i => ingredients.Contains(i.Nom.ToLower())));
        }

        var total = await requete.CountAsync(annulation);
        var recettes = await requete
            .OrderByDescending(r => r.DateCreation)
            .Skip((filtre.Page - 1) * filtre.TaillePage)
            .Take(filtre.TaillePage)
            .ToListAsync(annulation);

        return (recettes, total);
    }

    public async Task<IEnumerable<Recette>> ObtenirParAuteurAsync(Guid auteurId, CancellationToken annulation = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(annulation);
        return await ctx.Recettes
            .Include(r => r.RecetteCategories).ThenInclude(rc => rc.Categorie)
            .Include(r => r.RecetteEtiquettes).ThenInclude(re => re.Etiquette)
            .AsNoTracking()
            .Where(r => r.AuteurId == auteurId)
            .OrderByDescending(r => r.DateCreation)
            .ToListAsync(annulation);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken annulation = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(annulation);
        return await ctx.Recettes.AnyAsync(r => r.Id == id, annulation);
    }

    public Task<IEnumerable<Recette>> ObtenirPartagesesAvecAsync(Guid utilisateurId, CancellationToken annulation = default)
        => throw new NotSupportedException("Utiliser IUnitesDeTravail pour les opérations d'écriture.");
    public Task<Recette> AjouterAsync(Recette recette, CancellationToken annulation = default)
        => throw new NotSupportedException("Utiliser IUnitesDeTravail pour les opérations d'écriture.");
    public Task<Recette> ModifierAsync(Recette recette, CancellationToken annulation = default)
        => throw new NotSupportedException("Utiliser IUnitesDeTravail pour les opérations d'écriture.");
    public Task SupprimerAsync(Guid id, CancellationToken annulation = default)
        => throw new NotSupportedException("Utiliser IUnitesDeTravail pour les opérations d'écriture.");
    public Task PartagerAvecAsync(Guid recetteId, Guid utilisateurId, CancellationToken annulation = default)
        => throw new NotSupportedException("Utiliser IUnitesDeTravail pour les opérations d'écriture.");
    public Task RetirerPartageAsync(Guid recetteId, Guid utilisateurId, CancellationToken annulation = default)
        => throw new NotSupportedException("Utiliser IUnitesDeTravail pour les opérations d'écriture.");
}
