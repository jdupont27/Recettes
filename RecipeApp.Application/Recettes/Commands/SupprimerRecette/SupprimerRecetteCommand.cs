using MediatR;

namespace RecipeApp.Application.Recettes.Commands.SupprimerRecette;

public record SupprimerRecetteCommand(Guid Id, Guid AuteurId) : IRequest<Unit>;