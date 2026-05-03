using MediatR;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Commands.GenererLienPartage;

public class GenererLienPartageCommandHandler : IRequestHandler<GenererLienPartageCommand, string>
{
    private readonly IUnitesDeTravail _unitesDeTravail;

    public GenererLienPartageCommandHandler(IUnitesDeTravail unitesDeTravail)
    {
        _unitesDeTravail = unitesDeTravail;
    }

    public async Task<string> Handle(GenererLienPartageCommand commande, CancellationToken annulation)
    {
        await using var _ = _unitesDeTravail;

        var recette = await _unitesDeTravail.Recettes.ObtenirParIdAsync(commande.RecetteId, annulation)
            ?? throw new KeyNotFoundException($"Recette {commande.RecetteId} introuvable.");

        if (recette.AuteurId != commande.AuteurId)
            throw new UnauthorizedAccessException("Seul l'auteur peut générer un lien de partage.");

        // Réutiliser le token existant ou en générer un nouveau
        if (string.IsNullOrEmpty(recette.LienSecret))
            recette.LienSecret = Guid.NewGuid().ToString("N");

        await _unitesDeTravail.Recettes.ModifierAsync(recette, annulation);
        await _unitesDeTravail.SauvegarderAsync(annulation);

        return recette.LienSecret;
    }
}
