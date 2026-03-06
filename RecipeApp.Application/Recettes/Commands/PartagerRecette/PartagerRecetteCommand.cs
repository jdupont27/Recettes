using MediatR;

namespace RecipeApp.Application.Recettes.Commands.PartagerRecette;

public record PartagerRecetteCommand(Guid RecetteId, Guid UtilisateurId, Guid AuteurId) : IRequest<Unit>;