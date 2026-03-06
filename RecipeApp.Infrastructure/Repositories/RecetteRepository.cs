using Microsoft.EntityFrameworkCore;
using RecipeApp.Domain.Entities;
using RecipeApp.Domain.Enums;
using RecipeApp.Domain.Interfaces;
using RecipeApp.Infrastructure.Persistence;

namespace RecipeApp.Infrastructure.Repositories;

public class RecetteRepository : IRecetteRepository
{
    private readonly AppDbContext _contexte;

    public RecetteRepository(AppDbContext contexte)
    {
        _contexte = contexte;
    }

    public async Task<Recette?> ObtenirParIdAsync(Guid id, CancellationToken annulation = default)
    {
        return await _contexte.Recettes
            .Include(r => r.Ingredients.OrderBy(i => i.Ordre))
            .Include(r => r.Etapes.OrderBy(e => e.NumeroEtape))
            .Include(r => r.Partages)
            .Include(r => r.RecetteCategories).ThenInclude(rc => rc.Categorie)
            .Include(r => r.RecetteEtiquettes).ThenInclude(re => re.Etiquette)
            .FirstOrDefaultAsync(r => r.Id == id, annulation);
    }

    public async Task<(IEnumerable<Recette> Recettes, int Total)> RechercherAsync(FiltreRecette filtre, CancellationToken annulation = default)
    {
        var requete = _contexte.Recettes
            .Include(r => r.Ingredients)
            .Include(r => r.RecetteCategories).ThenInclude(rc => rc.Categorie)
            .Include(r => r.RecetteEtiquettes).ThenInclude(re => re.Etiquette)
            .Include(r => r.Partages)
            .AsQueryable();

        // Filtre de visibilité
        if (filtre.UtilisateurConnecteId.HasValue)
        {
            var userId = filtre.UtilisateurConnecteId.Value;
            requete = requete.Where(r =>
                (filtre.IncluirePubliques && r.Visibilite == Visibilite.Publique) ||
                r.AuteurId == userId ||
                (filtre.IncluirePartagees && r.Visibilite == Visibilite.Partagee &&
                    r.Partages.Any(p => p.UtilisateurId == userId)));
        }
        else
        {
            // Utilisateur non connecté : uniquement les recettes publiques
            requete = requete.Where(r => r.Visibilite == Visibilite.Publique);
        }

        // Filtre par auteur (mes recettes)
        if (filtre.AuteurId.HasValue)
            requete = requete.Where(r => r.AuteurId == filtre.AuteurId.Value);

        // Filtre texte libre (titre ou ingrédients)
        if (!string.IsNullOrWhiteSpace(filtre.TexteLibre))
        {
            var texte = filtre.TexteLibre.ToLower();
            requete = requete.Where(r =>
                r.Titre.ToLower().Contains(texte) ||
                (r.Description != null && r.Description.ToLower().Contains(texte)) ||
                r.Ingredients.Any(i => i.Nom.ToLower().Contains(texte)));
        }

        // Filtre par temps total maximum
        if (filtre.TempsMaxMinutes.HasValue)
            requete = requete.Where(r => r.TempsPreparation + r.TempsCuisson <= filtre.TempsMaxMinutes.Value);

        // Filtre par nombre de portions
        if (filtre.NombrePortions.HasValue)
            requete = requete.Where(r => r.NombrePortions == filtre.NombrePortions.Value);

        // Filtre par catégorie
        if (filtre.CategorieId.HasValue)
            requete = requete.Where(r => r.RecetteCategories.Any(rc => rc.CategorieId == filtre.CategorieId.Value));

        // Filtre par type de cuisine
        if (!string.IsNullOrWhiteSpace(filtre.TypeCuisine))
            requete = requete.Where(r => r.TypeCuisine != null && r.TypeCuisine.ToLower().Contains(filtre.TypeCuisine.ToLower()));

        // Filtre par difficulté
        if (filtre.Difficulte.HasValue)
            requete = requete.Where(r => r.Difficulte == filtre.Difficulte.Value);

        // Filtre par étiquettes (la recette doit posséder toutes les étiquettes sélectionnées)
        foreach (var etiquetteId in filtre.EtiquettesIds)
        {
            var id = etiquetteId;
            requete = requete.Where(r => r.RecetteEtiquettes.Any(re => re.EtiquetteId == id));
        }

        // Filtre par ingrédients disponibles (la recette contient au moins un de ces ingrédients)
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

    public async Task<Recette> AjouterAsync(Recette recette, CancellationToken annulation = default)
    {
        await _contexte.Recettes.AddAsync(recette, annulation);
        return recette;
    }

    public Task<Recette> ModifierAsync(Recette recette, CancellationToken annulation = default)
    {
        _contexte.Recettes.Update(recette);
        return Task.FromResult(recette);
    }

    public async Task SupprimerAsync(Guid id, CancellationToken annulation = default)
    {
        var recette = await _contexte.Recettes.FindAsync([id], annulation);
        if (recette != null)
            _contexte.Recettes.Remove(recette);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken annulation = default)
    {
        return await _contexte.Recettes.AnyAsync(r => r.Id == id, annulation);
    }

    public async Task<IEnumerable<Recette>> ObtenirParAuteurAsync(Guid auteurId, CancellationToken annulation = default)
    {
        return await _contexte.Recettes
            .Include(r => r.RecetteCategories).ThenInclude(rc => rc.Categorie)
            .Include(r => r.RecetteEtiquettes).ThenInclude(re => re.Etiquette)
            .Where(r => r.AuteurId == auteurId)
            .OrderByDescending(r => r.DateCreation)
            .ToListAsync(annulation);
    }

    public async Task<IEnumerable<Recette>> ObtenirPartagesesAvecAsync(Guid utilisateurId, CancellationToken annulation = default)
    {
        return await _contexte.Recettes
            .Include(r => r.Partages)
            .Include(r => r.RecetteCategories).ThenInclude(rc => rc.Categorie)
            .Where(r => r.Partages.Any(p => p.UtilisateurId == utilisateurId))
            .OrderByDescending(r => r.DateCreation)
            .ToListAsync(annulation);
    }

    public async Task PartagerAvecAsync(Guid recetteId, Guid utilisateurId, CancellationToken annulation = default)
    {
        var dejaPartage = await _contexte.PartagesRecette
            .AnyAsync(p => p.RecetteId == recetteId && p.UtilisateurId == utilisateurId, annulation);

        if (!dejaPartage)
        {
            await _contexte.PartagesRecette.AddAsync(new PartageRecette
            {
                RecetteId = recetteId,
                UtilisateurId = utilisateurId,
                DatePartage = DateTime.UtcNow
            }, annulation);
        }
    }

    public async Task RetirerPartageAsync(Guid recetteId, Guid utilisateurId, CancellationToken annulation = default)
    {
        var partage = await _contexte.PartagesRecette
            .FirstOrDefaultAsync(p => p.RecetteId == recetteId && p.UtilisateurId == utilisateurId, annulation);

        if (partage != null)
            _contexte.PartagesRecette.Remove(partage);
    }
}
