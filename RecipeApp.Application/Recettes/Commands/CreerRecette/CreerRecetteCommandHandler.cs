using MediatR;
using RecipeApp.Application.Common.Mappages;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Interfaces;
using RecipeApp.Domain.Entities;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Commands.CreerRecette;

public class CreerRecetteCommandHandler : IRequestHandler<CreerRecetteCommand, RecetteDto>
{
    private readonly IUnitesDeTravail _unitesDeTravail;
    private readonly IServiceFichiers _serviceFichiers;

    public CreerRecetteCommandHandler(IUnitesDeTravail unitesDeTravail, IServiceFichiers serviceFichiers)
    {
        _unitesDeTravail = unitesDeTravail;
        _serviceFichiers = serviceFichiers;
    }

    public async Task<RecetteDto> Handle(CreerRecetteCommand commande, CancellationToken annulation)
    {
        await using var _ = _unitesDeTravail;

        string? cheminImage = null;
        if (!string.IsNullOrEmpty(commande.ImageBase64) && !string.IsNullOrEmpty(commande.ImageTypeMime))
            cheminImage = await _serviceFichiers.SauvegarderImageAsync(commande.ImageBase64, commande.ImageTypeMime, annulation);

        var recette = new Recette
        {
            Id = Guid.NewGuid(),
            Titre = commande.Titre,
            Description = commande.Description,
            TempsPreparation = commande.TempsPreparation,
            TempsCuisson = commande.TempsCuisson,
            NombrePortions = commande.NombrePortions,
            Difficulte = commande.Difficulte,
            TypeCuisine = commande.TypeCuisine,
            Visibilite = commande.Visibilite,
            DateCreation = DateTime.UtcNow,
            AuteurId = commande.AuteurId,
            CheminImage = cheminImage,
            LienSecret = commande.Visibilite == Domain.Enums.Visibilite.Partagee
                ? Guid.NewGuid().ToString("N")
                : null
        };

        for (int i = 0; i < commande.Ingredients.Count; i++)
        {
            var dto = commande.Ingredients[i];
            recette.Ingredients.Add(new Ingredient
            {
                Id = Guid.NewGuid(),
                RecetteId = recette.Id,
                Nom = dto.Nom,
                Quantite = dto.Quantite,
                Unite = dto.Unite,
                Ordre = dto.Ordre > 0 ? dto.Ordre : i + 1
            });
        }

        for (int i = 0; i < commande.Etapes.Count; i++)
        {
            var dto = commande.Etapes[i];
            recette.Etapes.Add(new EtapeRecette
            {
                Id = Guid.NewGuid(),
                RecetteId = recette.Id,
                NumeroEtape = dto.NumeroEtape > 0 ? dto.NumeroEtape : i + 1,
                Description = dto.Description
            });
        }

        foreach (var categorieId in commande.CategoriesIds)
            recette.RecetteCategories.Add(new RecetteCategorie { RecetteId = recette.Id, CategorieId = categorieId });

        foreach (var etiquetteId in commande.EtiquettesIds)
            recette.RecetteEtiquettes.Add(new RecetteEtiquette { RecetteId = recette.Id, EtiquetteId = etiquetteId });

        await _unitesDeTravail.Recettes.AjouterAsync(recette, annulation);
        await _unitesDeTravail.SauvegarderAsync(annulation);

        return MappeurRecette.VersDto(recette);
    }
}
