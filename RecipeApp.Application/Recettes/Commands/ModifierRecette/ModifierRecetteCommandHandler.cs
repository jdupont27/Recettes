using MediatR;
using RecipeApp.Application.Common.Mappages;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Interfaces;
using RecipeApp.Domain.Entities;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Commands.ModifierRecette;

public class ModifierRecetteCommandHandler : IRequestHandler<ModifierRecetteCommand, RecetteDto>
{
    private readonly IUnitesDeTravail _unitesDeTravail;
    private readonly IServiceFichiers _serviceFichiers;

    public ModifierRecetteCommandHandler(IUnitesDeTravail unitesDeTravail, IServiceFichiers serviceFichiers)
    {
        _unitesDeTravail = unitesDeTravail;
        _serviceFichiers = serviceFichiers;
    }

    public async Task<RecetteDto> Handle(ModifierRecetteCommand commande, CancellationToken annulation)
    {
        await using var _ = _unitesDeTravail;

        var recette = await _unitesDeTravail.Recettes.ObtenirParIdAsync(commande.Id, annulation)
            ?? throw new KeyNotFoundException($"Recette {commande.Id} introuvable.");

        if (recette.AuteurId != commande.AuteurId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à modifier cette recette.");

        if (!string.IsNullOrEmpty(commande.ImageBase64) && !string.IsNullOrEmpty(commande.ImageTypeMime))
        {
            if (!string.IsNullOrEmpty(recette.CheminImage))
                await _serviceFichiers.SupprimerImageAsync(recette.CheminImage, annulation);
            recette.CheminImage = await _serviceFichiers.SauvegarderImageAsync(commande.ImageBase64, commande.ImageTypeMime, annulation);
        }

        recette.Titre = commande.Titre;
        recette.Description = commande.Description;
        recette.TempsPreparation = commande.TempsPreparation;
        recette.TempsCuisson = commande.TempsCuisson;
        recette.NombrePortions = commande.NombrePortions;
        recette.Difficulte = commande.Difficulte;
        recette.TypeCuisine = commande.TypeCuisine;
        recette.Visibilite = commande.Visibilite;
        recette.DateModification = DateTime.UtcNow;

        if (commande.Visibilite == Domain.Enums.Visibilite.Partagee && string.IsNullOrEmpty(recette.LienSecret))
            recette.LienSecret = Guid.NewGuid().ToString("N");
        else if (commande.Visibilite != Domain.Enums.Visibilite.Partagee)
            recette.LienSecret = null;

        // Diff catégories (clé composite — pas de Clear() pour éviter les conflits EF)
        var nouvellesCats = commande.CategoriesIds.ToHashSet();
        var anciennesCats = recette.RecetteCategories.Select(rc => rc.CategorieId).ToHashSet();
        foreach (var rc in recette.RecetteCategories.Where(rc => !nouvellesCats.Contains(rc.CategorieId)).ToList())
            recette.RecetteCategories.Remove(rc);
        foreach (var catId in nouvellesCats.Where(id => !anciennesCats.Contains(id)))
            recette.RecetteCategories.Add(new RecetteCategorie { RecetteId = recette.Id, CategorieId = catId });

        // Diff étiquettes
        var nouvellesEtiq = commande.EtiquettesIds.ToHashSet();
        var anciennesEtiq = recette.RecetteEtiquettes.Select(re => re.EtiquetteId).ToHashSet();
        foreach (var re in recette.RecetteEtiquettes.Where(re => !nouvellesEtiq.Contains(re.EtiquetteId)).ToList())
            recette.RecetteEtiquettes.Remove(re);
        foreach (var etiqId in nouvellesEtiq.Where(id => !anciennesEtiq.Contains(id)))
            recette.RecetteEtiquettes.Add(new RecetteEtiquette { RecetteId = recette.Id, EtiquetteId = etiqId });

        // Construire les nouvelles collections
        var nouveauxIngredients = commande.Ingredients
            .Select((dto, i) => new Ingredient
            {
                Id = Guid.NewGuid(),
                RecetteId = recette.Id,
                Nom = dto.Nom,
                Quantite = dto.Quantite,
                Unite = dto.Unite,
                Ordre = dto.Ordre > 0 ? dto.Ordre : i + 1
            }).ToList();

        var nouvellesEtapes = commande.Etapes
            .Select((dto, i) => new EtapeRecette
            {
                Id = Guid.NewGuid(),
                RecetteId = recette.Id,
                NumeroEtape = dto.NumeroEtape > 0 ? dto.NumeroEtape : i + 1,
                Description = dto.Description
            }).ToList();

        // DELETE SQL direct + AddRange pour ingrédients/étapes (évite DbUpdateConcurrencyException du tracking EF)
        await _unitesDeTravail.RemplacerCollectionsRecetteAsync(
            recette.Id, nouveauxIngredients, nouvellesEtapes, annulation);

        // Sauvegarder : Recette (scalaires + catégories + étiquettes) + nouveaux ingrédients/étapes
        await _unitesDeTravail.SauvegarderAsync(annulation);

        recette.Ingredients = nouveauxIngredients;
        recette.Etapes = nouvellesEtapes;

        return MappeurRecette.VersDto(recette);
    }
}
