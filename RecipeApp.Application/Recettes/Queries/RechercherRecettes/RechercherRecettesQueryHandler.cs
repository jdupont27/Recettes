using MediatR;
using RecipeApp.Application.Common.Mappages;
using RecipeApp.Application.DTOs;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Queries.RechercherRecettes;

public class RechercherRecettesQueryHandler : IRequestHandler<RechercherRecettesQuery, ResultatPagine<RecetteDto>>
{
    private readonly IRecetteRepository _recetteRepository;

    public RechercherRecettesQueryHandler(IRecetteRepository recetteRepository)
    {
        _recetteRepository = recetteRepository;
    }

    public async Task<ResultatPagine<RecetteDto>> Handle(RechercherRecettesQuery requete, CancellationToken annulation)
    {
        var filtre = new FiltreRecette
        {
            TexteLibre = requete.TexteLibre,
            TempsMaxMinutes = requete.TempsMaxMinutes,
            NombrePortions = requete.NombrePortions,
            CategorieId = requete.CategorieId,
            TypeCuisine = requete.TypeCuisine,
            Difficulte = requete.Difficulte,
            EtiquettesIds = requete.EtiquettesIds,
            IngredientsDisponibles = requete.IngredientsDisponibles,
            AuteurId = requete.AuteurId,
            IncluirePubliques = requete.IncluirePubliques,
            IncluirePartagees = requete.IncluirePartagees,
            UtilisateurConnecteId = requete.UtilisateurConnecteId,
            Page = requete.Page,
            TaillePage = requete.TaillePage
        };

        var (recettes, total) = await _recetteRepository.RechercherAsync(filtre, annulation);

        return new ResultatPagine<RecetteDto>
        {
            Elements = recettes.Select(r => MappeurRecette.VersDto(r)),
            Total = total,
            Page = requete.Page,
            TaillePage = requete.TaillePage
        };
    }
}
