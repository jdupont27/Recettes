using MediatR;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Commands.PartagerRecette;

public class PartagerRecetteCommandHandler : IRequestHandler<PartagerRecetteCommand, Unit>
{
    private readonly IUnitesDeTravail _unitesDeTravail;

    public PartagerRecetteCommandHandler(IUnitesDeTravail unitesDeTravail)
    {
        _unitesDeTravail = unitesDeTravail;
    }

    public async Task<Unit> Handle(PartagerRecetteCommand commande, CancellationToken annulation)
    {
        await using var _ = _unitesDeTravail;

        var recette = await _unitesDeTravail.Recettes.ObtenirParIdAsync(commande.RecetteId, annulation)
            ?? throw new KeyNotFoundException($"Recette {commande.RecetteId} introuvable.");

        if (recette.AuteurId != commande.AuteurId)
            throw new UnauthorizedAccessException("Seul l'auteur peut partager cette recette.");

        await _unitesDeTravail.Recettes.PartagerAvecAsync(commande.RecetteId, commande.UtilisateurId, annulation);
        await _unitesDeTravail.SauvegarderAsync(annulation);

        return Unit.Value;
    }
}
