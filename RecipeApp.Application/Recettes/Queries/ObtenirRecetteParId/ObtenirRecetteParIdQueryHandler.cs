using MediatR;
using RecipeApp.Application.Common.Mappages;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Interfaces;
using RecipeApp.Domain.Enums;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Queries.ObtenirRecetteParId;

public class ObtenirRecetteParIdQueryHandler : IRequestHandler<ObtenirRecetteParIdQuery, RecetteDto?>
{
    private readonly IRecetteRepository _recetteRepository;
    private readonly IServiceUtilisateurs _serviceUtilisateurs;

    public ObtenirRecetteParIdQueryHandler(IRecetteRepository recetteRepository, IServiceUtilisateurs serviceUtilisateurs)
    {
        _recetteRepository = recetteRepository;
        _serviceUtilisateurs = serviceUtilisateurs;
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

        if (!peutLire)
            return null;

        var nomAuteur = await _serviceUtilisateurs.ObtenirNomAsync(recette.AuteurId, annulation);
        return MappeurRecette.VersDto(recette, nomAuteur);
    }
}
