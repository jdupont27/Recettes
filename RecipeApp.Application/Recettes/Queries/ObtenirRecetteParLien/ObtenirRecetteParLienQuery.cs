using MediatR;
using RecipeApp.Application.DTOs;

namespace RecipeApp.Application.Recettes.Queries.ObtenirRecetteParLien;

public record ObtenirRecetteParLienQuery(string LienSecret) : IRequest<RecetteDto?>;
