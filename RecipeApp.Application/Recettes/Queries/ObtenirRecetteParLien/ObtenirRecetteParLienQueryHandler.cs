using MediatR;
using RecipeApp.Application.Common.Mappages;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Interfaces;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Queries.ObtenirRecetteParLien;

public class ObtenirRecetteParLienQueryHandler : IRequestHandler<ObtenirRecetteParLienQuery, RecetteDto?>
{
    private readonly IRecetteRepository _recetteRepository;
    private readonly IServiceUtilisateurs _serviceUtilisateurs;

    public ObtenirRecetteParLienQueryHandler(IRecetteRepository recetteRepository, IServiceUtilisateurs serviceUtilisateurs)
    {
        _recetteRepository = recetteRepository;
        _serviceUtilisateurs = serviceUtilisateurs;
    }

    public async Task<RecetteDto?> Handle(ObtenirRecetteParLienQuery requete, CancellationToken annulation)
    {
        var recette = await _recetteRepository.ObtenirParLienSecretAsync(requete.LienSecret, annulation);
        if (recette is null)
            return null;

        var nomAuteur = await _serviceUtilisateurs.ObtenirNomAsync(recette.AuteurId, annulation);
        return MappeurRecette.VersDto(recette, nomAuteur);
    }
}
