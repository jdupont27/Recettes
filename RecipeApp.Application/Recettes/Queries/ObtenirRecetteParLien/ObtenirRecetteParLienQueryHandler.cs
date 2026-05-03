using MediatR;
using RecipeApp.Application.Common.Mappages;
using RecipeApp.Application.DTOs;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Queries.ObtenirRecetteParLien;

public class ObtenirRecetteParLienQueryHandler : IRequestHandler<ObtenirRecetteParLienQuery, RecetteDto?>
{
    private readonly IRecetteRepository _recetteRepository;

    public ObtenirRecetteParLienQueryHandler(IRecetteRepository recetteRepository)
    {
        _recetteRepository = recetteRepository;
    }

    public async Task<RecetteDto?> Handle(ObtenirRecetteParLienQuery requete, CancellationToken annulation)
    {
        var recette = await _recetteRepository.ObtenirParLienSecretAsync(requete.LienSecret, annulation);
        return recette is null ? null : MappeurRecette.VersDto(recette);
    }
}
