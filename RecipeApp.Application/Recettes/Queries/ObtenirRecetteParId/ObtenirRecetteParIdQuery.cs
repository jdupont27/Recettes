using MediatR;
using RecipeApp.Application.DTOs;

namespace RecipeApp.Application.Recettes.Queries.ObtenirRecetteParId;

public record ObtenirRecetteParIdQuery(Guid Id, Guid? UtilisateurConnecteId) : IRequest<RecetteDto?>;