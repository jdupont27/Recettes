using RecipeApp.Application.DTOs;
using RecipeApp.Domain.Entities;

namespace RecipeApp.Application.Common.Mappages;

/// <summary>Utilitaires de conversion entre entités Domain et DTOs.</summary>
public static class MappeurRecette
{
    public static RecetteDto VersDto(Recette recette, string? nomAuteur = null) => new()
    {
        Id = recette.Id,
        Titre = recette.Titre,
        Description = recette.Description,
        TempsPreparation = recette.TempsPreparation,
        TempsCuisson = recette.TempsCuisson,
        NombrePortions = recette.NombrePortions,
        Difficulte = recette.Difficulte,
        TypeCuisine = recette.TypeCuisine,
        Visibilite = recette.Visibilite,
        DateCreation = recette.DateCreation,
        AuteurId = recette.AuteurId,
        NomAuteur = nomAuteur,
        CheminImage = recette.CheminImage,
        Ingredients = recette.Ingredients
            .OrderBy(i => i.Ordre)
            .Select(i => new IngredientDto
            {
                Id = i.Id,
                Nom = i.Nom,
                Quantite = i.Quantite,
                Unite = i.Unite,
                Ordre = i.Ordre
            }).ToList(),
        Etapes = recette.Etapes
            .OrderBy(e => e.NumeroEtape)
            .Select(e => new EtapeDto
            {
                Id = e.Id,
                NumeroEtape = e.NumeroEtape,
                Description = e.Description
            }).ToList(),
        Categories = recette.RecetteCategories
            .Select(rc => new CategorieDto
            {
                Id = rc.CategorieId,
                Nom = rc.Categorie?.Nom ?? string.Empty
            }).ToList(),
        Etiquettes = recette.RecetteEtiquettes
            .Select(re => new EtiquetteDto
            {
                Id = re.EtiquetteId,
                Nom = re.Etiquette?.Nom ?? string.Empty
            }).ToList()
    };
}