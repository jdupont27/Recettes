using MediatR;
using RecipeApp.Application.Common.Mappages;
using RecipeApp.Application.DTOs;
using RecipeApp.Domain.Enums;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Queries.ObtenirRecetteParId;

public class ObtenirRecetteParIdQueryHandler : IRequestHandler<ObtenirRecetteParIdQuery, RecetteDto?>
{
    private readonly IRecetteRepository _recetteRepository;

    public ObtenirRecetteParIdQueryHandler(IRecetteRepository recetteRepository)
    {
        _recetteRepository = recetteRepository;
    }

    public async Task<RecetteDto?> Handle(ObtenirRecetteParIdQuery requete, CancellationToken annulation)
    {
        var recette = await _recetteRepository.ObtenirParIdAsync(requete.Id, annulation);
        if (recette == null)
            return null;

        // Vérifier les droits de lecture selon la visibilité
        var peutLire = recette.Visibilite switch
        {
            Visibilite.Publique => true,
            Visibilite.Privee => recette.AuteurId == requete.UtilisateurConnecteId,
            Visibilite.Partagee => recette.AuteurId == requete.UtilisateurConnecteId
                || recette.Partages.Any(p => p.UtilisateurId == requete.UtilisateurConnecteId),
            _ => false
        };

        return peutLire ? MappeurRecette.VersDto(recette) : null;
    }
}
