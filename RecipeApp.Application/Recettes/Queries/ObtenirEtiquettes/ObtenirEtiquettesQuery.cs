using MediatR;
using RecipeApp.Application.DTOs;

namespace RecipeApp.Application.Recettes.Queries.ObtenirEtiquettes;

public record ObtenirEtiquettesQuery : IRequest<List<EtiquetteDto>>;
