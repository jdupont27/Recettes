using MediatR;
using RecipeApp.Application.Common.Mappages;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Interfaces;
using RecipeApp.Domain.Entities;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Commands.ModifierRecette;

public class ModifierRecetteCommandHandler : IRequestHandler<ModifierRecetteCommand, RecetteDto>
{
    private readonly IRecetteRepository _recetteRepository;
    private readonly IServiceFichiers _serviceFichiers;
    private readonly IUnitOfWork _unitOfWork;

    public ModifierRecetteCommandHandler(
        IRecetteRepository recetteRepository,
        IServiceFichiers serviceFichiers,
        IUnitOfWork unitOfWork)
    {
        _recetteRepository = recetteRepository;
        _serviceFichiers = serviceFichiers;
        _unitOfWork = unitOfWork;
    }

    public async Task<RecetteDto> Handle(ModifierRecetteCommand commande, CancellationToken annulation)
    {
        var recette = await _recetteRepository.ObtenirParIdAsync(commande.Id, annulation)
            ?? throw new KeyNotFoundException($"Recette {commande.Id} introuvable.");

        if (recette.AuteurId != commande.AuteurId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à modifier cette recette.");

        // Mettre à jour l'image si une nouvelle est fournie
        if (!string.IsNullOrEmpty(commande.ImageBase64) && !string.IsNullOrEmpty(commande.ImageTypeMime))
        {
            if (!string.IsNullOrEmpty(recette.CheminImage))
                await _serviceFichiers.SupprimerImageAsync(recette.CheminImage, annulation);

            recette.CheminImage = await _serviceFichiers.SauvegarderImageAsync(commande.ImageBase64, commande.ImageTypeMime, annulation);
        }

        // Mettre à jour les propriétés scalaires
        recette.Titre = commande.Titre;
        recette.Description = commande.Description;
        recette.TempsPreparation = commande.TempsPreparation;
        recette.TempsCuisson = commande.TempsCuisson;
        recette.NombrePortions = commande.NombrePortions;
        recette.Difficulte = commande.Difficulte;
        recette.TypeCuisine = commande.TypeCuisine;
        recette.Visibilite = commande.Visibilite;
        recette.DateModification = DateTime.UtcNow;

        // Remplacer intégralement les ingrédients
        recette.Ingredients.Clear();
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

        // Remplacer intégralement les étapes
        recette.Etapes.Clear();
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

        // Remplacer les catégories et étiquettes
        recette.RecetteCategories.Clear();
        foreach (var categorieId in commande.CategoriesIds)
            recette.RecetteCategories.Add(new RecetteCategorie { RecetteId = recette.Id, CategorieId = categorieId });

        recette.RecetteEtiquettes.Clear();
        foreach (var etiquetteId in commande.EtiquettesIds)
            recette.RecetteEtiquettes.Add(new RecetteEtiquette { RecetteId = recette.Id, EtiquetteId = etiquetteId });

        await _recetteRepository.ModifierAsync(recette, annulation);
        await _unitOfWork.SauvegarderAsync(annulation);

        return MappeurRecette.VersDto(recette);
    }
}