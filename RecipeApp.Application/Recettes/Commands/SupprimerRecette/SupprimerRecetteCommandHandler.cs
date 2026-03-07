using MediatR;
using RecipeApp.Application.Interfaces;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Commands.SupprimerRecette;

public class SupprimerRecetteCommandHandler : IRequestHandler<SupprimerRecetteCommand, Unit>
{
    private readonly IUnitesDeTravail _unitesDeTravail;
    private readonly IServiceFichiers _serviceFichiers;

    public SupprimerRecetteCommandHandler(IUnitesDeTravail unitesDeTravail, IServiceFichiers serviceFichiers)
    {
        _unitesDeTravail = unitesDeTravail;
        _serviceFichiers = serviceFichiers;
    }

    public async Task<Unit> Handle(SupprimerRecetteCommand commande, CancellationToken annulation)
    {
        await using var _ = _unitesDeTravail;

        var recette = await _unitesDeTravail.Recettes.ObtenirParIdAsync(commande.Id, annulation)
            ?? throw new KeyNotFoundException($"Recette {commande.Id} introuvable.");

        if (recette.AuteurId != commande.AuteurId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à supprimer cette recette.");

        if (!string.IsNullOrEmpty(recette.CheminImage))
            await _serviceFichiers.SupprimerImageAsync(recette.CheminImage, annulation);

        await _unitesDeTravail.Recettes.SupprimerAsync(commande.Id, annulation);
        await _unitesDeTravail.SauvegarderAsync(annulation);

        return Unit.Value;
    }
}
