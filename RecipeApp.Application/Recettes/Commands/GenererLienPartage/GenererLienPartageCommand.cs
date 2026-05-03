using MediatR;

namespace RecipeApp.Application.Recettes.Commands.GenererLienPartage;

public record GenererLienPartageCommand(Guid RecetteId, Guid AuteurId) : IRequest<string>;
