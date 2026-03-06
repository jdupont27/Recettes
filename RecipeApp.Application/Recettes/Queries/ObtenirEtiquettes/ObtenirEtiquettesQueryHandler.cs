using MediatR;
using RecipeApp.Application.DTOs;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Queries.ObtenirEtiquettes;

public class ObtenirEtiquettesQueryHandler : IRequestHandler<ObtenirEtiquettesQuery, List<EtiquetteDto>>
{
    private readonly IEtiquetteRepository _etiquetteRepository;

    public ObtenirEtiquettesQueryHandler(IEtiquetteRepository etiquetteRepository)
    {
        _etiquetteRepository = etiquetteRepository;
    }

    public async Task<List<EtiquetteDto>> Handle(ObtenirEtiquettesQuery requete, CancellationToken annulation)
    {
        var etiquettes = await _etiquetteRepository.ObtenirToutesAsync(annulation);
        return etiquettes.Select(e => new EtiquetteDto { Id = e.Id, Nom = e.Nom }).ToList();
    }
}
